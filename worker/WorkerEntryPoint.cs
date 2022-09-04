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
        var args = new[]
        {
            (15, 15),
            (17, 17),
            (19, 19)
        };

        var works = Enumerable.Range(1, 25).SelectMany(problemId => args.Select(a => new { problemId, rows = a.Item1, cols = a.Item2 })).ToArray();

        Console.WriteLine($"Total: {works.Length}");
        var processed = 0;

        Parallel.ForEach(works, w =>
        {
            var problemId = w.problemId;
            var rows = w.rows;
            var cols = w.cols;
            var res = GridGuidedPainterRunner.Solve(problemId, rows, cols);
            var score = res.Score;
            SolutionRepo.Submit(
                new ContestSolution(
                    problemId,
                    score,
                    res.Moves.StrJoin("\n"),
                    new SolverMeta{Description = $"{rows}*{cols} colTolerance={res.ColorTolerance}"},
                    "GridGuidedPainter"));

            var incremented = Interlocked.Increment(ref processed);
            var prevBestScore = SolutionRepo.GetBestSolutionByProblemId(problemId).GetAwaiter().GetResult()!.ScoreEstimated;
            if (score < prevBestScore)
                Console.WriteLine($"BEST! {incremented}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} ");
            else
                Console.WriteLine($"{incremented}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols}");
        });
    }
}
