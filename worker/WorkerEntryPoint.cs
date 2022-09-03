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
            (11, 9),
            (9, 11),
            (13, 11),
            (11, 13),
        };

        var works = Enumerable.Range(1, 25).SelectMany(problemId => args.Select(a => new { problemId, rows = a.Item1, cols = a.Item2 })).ToArray();

        Console.WriteLine($"Total: {works.Length}");
        var processed = 0;

        Parallel.ForEach(works, w =>
        {
            var problemId = w.problemId;
            var rows = w.rows;
            var cols = w.cols;
            var moves = GridGuidedPainterRunner.Solve(problemId, rows, cols);
            var score = Screen.LoadProblem(problemId).CalculateScore(moves);
            SolutionRepo.Submit(
                new ContestSolution(
                    problemId,
                    score,
                    moves.StrJoin("\n"),
                    new SolverMeta{Description = $"{rows}*{cols}"},
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
