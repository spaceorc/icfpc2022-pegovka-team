using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;
// ReSharper disable InconsistentNaming

namespace lib.db;

public class ContestSolution
{
    public Guid Id;
    public long? SubmissionId;
    public long ProblemId;
    public string SolverId;
    public long ScoreEstimated;
    public long? ScoreServer;
    public string Solution;
    public SolverMeta SolverMeta;
    public DateTime SolvedAt;
    public DateTime? SubmittedAt;

    public ContestSolution(long problemId, long scoreEstimated, string solution, SolverMeta solverMeta, DateTime solvedAt,  string solverId)
    {
        Id = Guid.NewGuid();
        ProblemId = problemId;
        ScoreEstimated = scoreEstimated;
        Solution = solution;
        SolverMeta = solverMeta;
        SolvedAt = solvedAt;
        SolverId = solverId;
    }

    public override string ToString()
    {
        return $"{nameof(Id)}: {Id}, {nameof(SubmissionId)}: {SubmissionId}, {nameof(ProblemId)}: {ProblemId}, {nameof(SolverId)}: {SolverId}, {nameof(ScoreEstimated)}: {ScoreEstimated}, {nameof(ScoreServer)}: {ScoreServer}, {nameof(Solution)}: {Solution}, {nameof(SolverMeta)}: {SolverMeta}, {nameof(SolvedAt)}: {SolvedAt}, {nameof(SubmittedAt)}: {SubmittedAt}";
    }

    public ContestSolution(ResultSet.Row row)
    {
        var id = (string?) row["id"] ?? throw new ArgumentException();
        var solverMeta = row["solver_meta"].GetOptionalJson() ?? throw new ArgumentException();
        Id = Guid.Parse(id);
        ProblemId = (long?) row["problem_id"] ?? throw new ArgumentException();
        ScoreEstimated = (long?) row["score_estimated"] ?? throw new ArgumentException();
        Solution = (string?) row["solution"] ?? throw new ArgumentException();
        SolverMeta = solverMeta.FromJson<SolverMeta>();
        SolvedAt = (DateTime?) row["solved_at"] ?? throw new ArgumentException();
        SolverId = (string?) row["solver_id"] ?? throw new ArgumentException();

        SubmissionId = (long?) row["submission_id"];
        ScoreServer = (long?) row["score_server"];
        SubmittedAt = (DateTime?) row["submitted_at"];
    }
}

public class SolverMeta
{
    public long Previous_Score;
    public string? Previous_SolverName;

    public SolverMeta()
    {

    }

    public SolverMeta(long previousScore, string previousSolverName)
    {
        Previous_Score = previousScore;
        Previous_SolverName = previousSolverName;
    }

    public override string ToString()
    {
        return $"{nameof(Previous_Score)}: {Previous_Score}, {nameof(Previous_SolverName)}: {Previous_SolverName}";
    }

    public string ToJson()
    {
        return JsonExtensions.ToJson(this);
    }
}

public static class SolutionRepo
{
    public static async Task Submit(ContestSolution solution)
    {
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $id AS Utf8;
                DECLARE $problem_id AS Int64;
                DECLARE $score_estimated AS Int64;
                DECLARE $score_server AS Int64?;
                DECLARE $solution AS Utf8;
                DECLARE $solved_at AS Datetime;
                DECLARE $solver_id AS Utf8;
                DECLARE $solver_meta AS Json;
                DECLARE $submission_id AS Int64?;
                DECLARE $submitted_at AS Datetime?;
                UPSERT INTO Solutions (id, problem_id, score_estimated, score_server,solution, solved_at, solver_id, solver_meta, submission_id, submitted_at) VALUES ($id, $problem_id, $score_estimated, $score_server, $solution, $solved_at, $solver_id, $solver_meta, $submission_id, $submitted_at)",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$id", YdbValue.MakeUtf8(solution.Id.ToString())},
                    { "$problem_id", YdbValue.MakeInt64(solution.ProblemId)},
                    { "$score_estimated", YdbValue.MakeInt64(solution.ScoreEstimated)},
                    { "$score_server", solution.ScoreServer == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Int64) : YdbValue.MakeOptional(YdbValue.MakeInt64((long) solution.ScoreServer))},
                    { "$solution", YdbValue.MakeUtf8(solution.Solution)},
                    { "$solved_at", YdbValue.MakeDatetime(solution.SolvedAt)},
                    { "$solver_id", YdbValue.MakeUtf8(solution.SolverId)},
                    { "$solver_meta", YdbValue.MakeJson(solution.SolverMeta.ToJson())},
                    { "$submission_id", solution.SubmissionId == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Int64) : YdbValue.MakeOptional(YdbValue.MakeInt64((long) solution.SubmissionId))},
                    { "$submitted_at", solution.SubmittedAt == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Datetime) : YdbValue.MakeOptional(YdbValue.MakeDatetime((DateTime) solution.SubmittedAt))},
                }
            ));
        response.Status.EnsureSuccess();
    }

    public static async Task<List<(long problemId, long score)>> GetBestScoreByProblemId()
    {

        var ans = new List<(long, long)>();
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $problem_id AS Int64;
                DECLARE $score_estimated AS Double;

                SELECT problem_id, min(score_estimated) as score from Solutions Group by problem_id;",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue> {}

            ));
        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse)response;
        foreach (var row in queryResponse.Result.ResultSets[0].Rows)
        {
            var problemId = (long?) row["problem_id"] ?? throw new ArgumentException();
            var scoreEstimated = (long?) row["score"] ?? throw new ArgumentException();
            ans.Add(new (problemId, scoreEstimated));
        }

        return ans;
    }

    public static async Task<ContestSolution> GetSolutionByIdAndScore(long problemId, long scoreEstimated)
    {
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $problem_id AS Int64;
                DECLARE $score_estimated AS Int64;

                SELECT * from Solutions where problem_id=$problem_id and score_estimated=$score_estimated limit 1",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$problem_id", YdbValue.MakeInt64(problemId)},
                    { "$score_estimated", YdbValue.MakeInt64(scoreEstimated)},
                }

            ));
        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse) response;
        var row = queryResponse.Result.ResultSets[0].Rows.First();

        var ans = new ContestSolution(row);
        return ans;
    }

    public static async Task<ContestSolution> GetBestSolutionBySolverId(long problemId, string solverId)
    {
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $problem_id AS Int64;
                DECLARE $solver_id AS Utf8;

                SELECT * from Solutions where problem_id=$problem_id and solver_id=$solver_id order by score_estimated limit 1",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$problem_id", YdbValue.MakeInt64(problemId)},
                    { "$solver_id", YdbValue.MakeUtf8(solverId)},
                }

            ));
        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse) response;
        var row = queryResponse.Result.ResultSets[0].Rows.First();

        var ans = new ContestSolution(row);
        return ans;
    }

    public static async Task<string[]> GetAllSolvers(long problemId)
    {
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                DECLARE $problem_id AS Int64;

                SELECT distinct solver_id from Solutions where problem_id=$problem_id",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>
                {
                    { "$problem_id", YdbValue.MakeInt64(problemId)},
                }

            ));
        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse) response;
        return queryResponse.Result.ResultSets[0].Rows.Select(x => (string?)x["solver_id"] ?? throw new Exception("WTF")).ToArray();
    }

    public static async Task<long[]> GetAllProblems()
    {
        var client = await CreateTableClient();
        var response = await client.SessionExec(async session =>

            await session.ExecuteDataQuery(
                query: @"
                SELECT distinct problem_id from Solutions",
                txControl: TxControl.BeginSerializableRW().Commit(),
                parameters: new Dictionary<string, YdbValue>()));
        response.Status.EnsureSuccess();
        var queryResponse = (ExecuteDataQueryResponse) response;
        return queryResponse.Result.ResultSets[0].Rows.Select(x => (long?)x["problem_id"] ?? throw new Exception("WTF")).ToArray();
    }

    private static async Task<TableClient> CreateTableClient()
    {
        var settings = new Settings();
        var config = new DriverConfig(
            endpoint: settings.YdbEndpoint,
            database: settings.YdbDatabase,
            credentials: new ServiceAccountProvider(settings.YandexCloudKeyFile),
            customServerCertificate: YcCerts.GetDefaultServerCertificate()
        );

        var driver = new Driver(
            config: config,
            loggerFactory: new NullLoggerFactory()
        );

        await driver.Initialize();

        return new TableClient(driver, new TableClientConfig());
    }
}
