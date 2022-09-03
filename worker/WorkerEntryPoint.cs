using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;

namespace worker;

public static class WorkerEntryPoint
{
    public static void Main()
    {
        // var releaseTag = EnvironmentVariables.Get("PEGOVKA_RELEASE_TAG");
        // var shardingToken = EnvironmentVariables.Get("PEGOVKA_SHARDING_TOKEN");
        // var timeoutMinutes = EnvironmentVariables.TryGet("PEGOVKA_TIMEOUT_MINUTES", int.Parse, 15);
        // Console.Out.WriteLine($"Worker '{releaseTag}' is processing shard: {shardingToken}");
        //
        // // определить задачу по shardingToken
        // // решить задачу
        // // сохранить решение задачи в БД

        var processed = 0;

        Parallel.ForEach(Enumerable.Range(1, 25), problemId =>
        {
            var moves = GridGuidedPainterRunner.Solve(problemId);
            var score = Screen.LoadProblem(problemId).CalculateScore(moves);
            SolutionRepo.Submit(
                new ContestSolution(
                    problemId,
                    score,
                    moves.StrJoin("\n"),
                    new SolverMeta(),
                    "GridGuidedPainter"));

            var incremented = Interlocked.Increment(ref processed);
            Console.WriteLine($"{incremented}/25 submitted score={score}");
        });
    }
}
