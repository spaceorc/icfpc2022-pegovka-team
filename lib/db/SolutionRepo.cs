using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Ydb.Sdk;
using Ydb.Sdk.Table;
using Ydb.Sdk.Value;
using Ydb.Sdk.Yc;

namespace lib.db;

public class ContestSolution
{
    public Guid Id;
    public long? SubmissionId;
    public long ProblemId;
    public string SolverId;
    public double ScoreEstimated;
    public double? ScoreServer;
    public string Solution;
    public SolverMeta SolverMeta;
    public DateTime SolvedAt;
    public DateTime? SubmittedAt;

    public ContestSolution(long problemId, double scoreEstimated, string solution, SolverMeta solverMeta, DateTime solvedAt,  string solverId)
    {
        Id = Guid.NewGuid();
        ProblemId = problemId;
        ScoreEstimated = scoreEstimated;
        Solution = solution;
        SolverMeta = solverMeta;
        SolvedAt = solvedAt;
        SolverId = solverId;
    }
}

public class SolverMeta
{

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
                DECLARE $score_estimated AS Double;
                DECLARE $score_server AS Double?;
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
                    { "$score_estimated", YdbValue.MakeDouble(solution.ScoreEstimated)},
                    { "$score_server", solution.ScoreServer == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Double) : YdbValue.MakeOptional(YdbValue.MakeDouble((double) solution.ScoreServer))},
                    { "$solution", YdbValue.MakeUtf8(solution.Solution)},
                    { "$solved_at", YdbValue.MakeDatetime(solution.SolvedAt)},
                    { "$solver_id", YdbValue.MakeUtf8(solution.SolverId)},
                    { "$solver_meta", YdbValue.MakeJson("{}")},
                    { "$submission_id", solution.SubmissionId == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Int64) : YdbValue.MakeOptional(YdbValue.MakeInt64((long) solution.SubmissionId))},
                    { "$submitted_at", solution.SubmittedAt == null ? YdbValue.MakeEmptyOptional(YdbTypeId.Datetime) : YdbValue.MakeOptional(YdbValue.MakeDatetime((DateTime) solution.SubmittedAt))},
                }
            ));
        response.Status.EnsureSuccess();
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
