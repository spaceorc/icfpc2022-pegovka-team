using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lib;
using lib.db;
using lib.Enchancers;

namespace worker;

public static class WorkerEntryPoint
{
    public static async Task Main()
    {
        // var releaseTag = EnvironmentVariables.Get("PEGOVKA_RELEASE_TAG");
        // var shardingToken = EnvironmentVariables.Get("PEGOVKA_SHARDING_TOKEN");
        // var timeoutMinutes = EnvironmentVariables.TryGet("PEGOVKA_TIMEOUT_MINUTES", int.Parse, 15);
        // Console.Out.WriteLine($"Worker '{releaseTag}' is processing shard: {shardingToken}");
        //
        // // определить задачу по shardingToken
        // // решить задачу
        // // сохранить решение задачи в БД


        var works = new List<ContestSolution>();
        foreach (var problemId in await SolutionRepo.GetAllProblems())
        {
            var solvers = await SolutionRepo.GetAllSolvers(problemId);
            foreach (var solverId in solvers)
            {
                if (!solverId.Contains("manual"))
                    continue;

                if (solverId.EndsWith("-enchanced"))
                    continue;

                var sol = await SolutionRepo.GetBestSolutionBySolverId(problemId, solverId);
                works.Add(sol);
                Console.WriteLine($"Existing solution: {sol.ProblemId} {sol.SolverId}");
            }
        }

        Console.WriteLine($"Total works count: {works.Count}");

        var processed = 0;

        Parallel.ForEach(works, sol =>
        {
            var moves = Moves.Parse(sol.Solution);
            var problem = Screen.LoadProblem((int)sol.ProblemId);
            var originalScore = problem.CalculateScore(moves);
            moves = Enchancer.Enchance(problem, moves);
            var enchancedScore = problem.CalculateScore(moves);
            if (enchancedScore < originalScore)
            {
                SolutionRepo.Submit(new ContestSolution(
                    sol.ProblemId,
                    enchancedScore,
                    moves.StrJoin("\n"),
                    new SolverMeta(),
                    DateTime.UtcNow,
                    sol.SolverId + "-enchanced"
                )).GetAwaiter().GetResult();
                var incremented = Interlocked.Increment(ref processed);
                Console.WriteLine($"{incremented}/{works.Count} enchanced {sol.ProblemId} {sol.SolverId}. {originalScore} -> {enchancedScore}");
            }
        });
    }
}
