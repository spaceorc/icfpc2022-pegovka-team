using lib;
using lib.api;
using lib.db;
using Spectre.Console;

namespace submitter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var api = new Api();
            var r = api.FetchProblem(1);
            // Console.WriteLine(r.Result);
            var submissionRepo = new SubmissionRepo(new Settings());

            while (true)
            {
                var logMessages = new List<string>();

                var footerTable = new Table().HideHeaders();
                footerTable.Border = TableBorder.None;
                footerTable.AddColumns("", "");
                footerTable.AddRow(
                    new Markup(string.Join("\n", logMessages)),
                    new FigletText("Pegovka Solutions").Color(Color.Aqua).Alignment(Justify.Right));
                AnsiConsole.Write(footerTable);

                AnsiConsole.Progress()
                    .Columns(
                        new SpinnerColumn(),
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn())
                    .Start(ctx =>
                    {
                        var wait = ctx.AddTask("Waiting for submission timeout to expire");

                        for (var i = 0; i < 100; i++)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(3));
                            wait.Increment(1);
                        }
                    });
            }
        }
        private static void RefreshDashboard(List<string> logMessages, Api api, SubmissionRepo submissionRepo)
        {
            // var worldBestDislikes = new ProblemTableApi().FetchDislikesAsync().Result.ToDictionary(t => t.ProblemId);

            var problemIds = ScreenRepo.GetProblemIds();
            var submitterRepo = new SubmissionRepo(new Settings());

            foreach (var id in problemIds)
            {

                var problem = ScreenRepo.GetProblem(id);
                // var state = new State();

                var result = api.PostSolution(id, "");
                Console.WriteLine(result);

            }

            Console.Clear();
            AnsiConsole.Write("");
        }
    }
}
