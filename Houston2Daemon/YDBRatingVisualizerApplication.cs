using Google.Apis.Sheets.v4.Data;
using lib;
using lib.db;
using Vostok.Applications.Scheduled;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Logging.Abstractions;

namespace Houston2Daemon;

[RequiresSecretConfiguration(typeof(Secrets))]
public class YDBRatingVisualizerApplication : VostokScheduledApplication
{
    private const int SolverTypes = 3;
    private const string E = "-enchanced";
    private const string E2 = $"-2{E}";
    private List<string> headers = new() { "ProblemId", "Type", "Top1 score", "Top1 solver", "Top2 score", "Top2 solver", "Top3 score", "Top3 solver" };
    private GSheetClient gsClient = new();
    private Secrets? secrets;

    public override void Setup(IScheduledActionsBuilder builder, IVostokHostingEnvironment environment)
    {
        secrets = environment.SecretConfigurationProvider.Get<Secrets>();

        builder.Schedule(
            "Update",
            Scheduler.Periodical(() => 1.Minutes()),
            () => PerformIteration(environment));
    }

    private void PerformIteration(IVostokHostingEnvironment environment)
    {
        environment.Log.Info("Start updating..");
        var bestStats = SolutionRepo.GetAllBestStats().GetAwaiter().GetResult();
        var spreadSheet = gsClient.GetSpreadsheet(secrets!.SpreadSheetId!);
        var sheet = spreadSheet.GetSheetByName(secrets.SpreadSheetName!);
        var data = new List<List<string>>{headers};
        var baseDict = new Dictionary<long, List<(long?, string?)>>();
        var e1Dict = new Dictionary<long, List<(long?, string?)>>();
        var e2Dict = new Dictionary<long, List<(long?, string?)>>();

        foreach (var (prId, score, solverId) in bestStats)
        {
            if (solverId is null)
                continue;

            if (solverId.EndsWith(E2))
            {
                var el = (score, id: solverId.Replace(E2, ""));
                e2Dict.AddOrUpdate(prId, new List<(long?, string?)>{el}, upd => upd.Add(el));
            }
            else if (solverId.EndsWith(E))
            {
                var el = (score, id: solverId.Replace(E, ""));
                e1Dict.AddOrUpdate(prId, new List<(long?, string?)>{el}, upd => upd.Add(el));
            }
            else
            {
                var el = (score, id: solverId);
                baseDict.AddOrUpdate(prId, new List<(long?, string?)>{el}, upd => upd.Add(el));
            }
        }

        foreach (var prId in baseDict.Keys)
        {
            data.Add(FormatLine(prId, new List<string> {prId.ToString()}, baseDict, "Base"));
            data.Add(FormatLine(prId, new List<string>{" "}, e1Dict, "Ench1"));
            data.Add(FormatLine(prId, new List<string>{" "}, e2Dict, "Ench2"));
        }

        var builder = sheet.Edit();
        builder.ClearAll().WriteRange((0, 0), data);
        foreach (var pr in baseDict.Keys)
        {
            var rowNumber = (int) pr * SolverTypes;
            var rangeStart = (rowNumber, 0);
            var rangeEnd = (rowNumber, headers.Count - 1);
            builder.AddBottomBorders(rangeStart, rangeEnd, new Color());
        }
        environment.Log.Info("Start executing builder..");

        builder.Execute();
        environment.Log.Info("Finished updating");
    }

    public static List<string> FormatLine(long prId, List<string> result, Dictionary<long, List<(long?, string?)>> dict, string solverType)
    {
        var top3 = dict.ContainsKey(prId)
            ? dict[prId]
                .Where(e => e.Item1.HasValue && e.Item2 != null)
                .OrderBy(e => e.Item1)
                .Take(3)
                .ToArray()
            : Array.Empty<(long?, string?)>();

        result.Add(solverType);
        foreach (var (score, solver) in top3)
        {
            result.Add(score!.Value.ToString());
            result.Add(solver!);
        }
        return result;
    }
}
