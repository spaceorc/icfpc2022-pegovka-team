using System;
using System.Collections.Generic;
using System.IO;
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
            // (7, 13),
            //(20, 20),
            (40, 40),
        };

        var works = Enumerable.Range(1, 40)
            .SelectMany(problemId => args.Select(a => new { problemId, rows = a.Item1, cols = a.Item2 }))
            .SelectMany(a => Enumerable.Range(0, 8).Select(o => new { a.problemId, a.rows, a.cols, orientation = o }))
            .SelectMany(a => new[]{false, true}.Select(prep => new { a.problemId, a.rows, a.cols, a.orientation, useSwapperPreprocessor = prep }))
            .Where(x => x.problemId is <= 25 or >= 36)
            .Where(x => x.orientation is 0 or 1)
            .Where(x => x.useSwapperPreprocessor)
            .ToArray();

        Console.WriteLine($"Total: {works.Length}");
        var current = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 90; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                while (true)
                {
                    var localCurrent = Interlocked.Increment(ref current) - 1;
                    if (localCurrent >= works.Length)
                        return;

                    var w = works[localCurrent];
                    var problemId = w.problemId;
                    var rows = w.rows;
                    var cols = w.cols;

                    var res = GridGuidedPainterRunner.Solve(problemId, rows, cols, w.orientation, w.useSwapperPreprocessor);
                    var score = res.Score;

                    long prevBestScore = -1;
                    try
                    {
                        prevBestScore = SolutionRepo.GetBestSolutionByProblemId(problemId).GetAwaiter().GetResult()!.ScoreEstimated;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (score < prevBestScore)
                        Console.WriteLine($"BEST! {localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} o={w.orientation} prep={w.useSwapperPreprocessor}");
                    else
                        Console.WriteLine($"{localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} o={w.orientation} prep={w.useSwapperPreprocessor}");


                    // File.WriteAllText(Path.Combine(FileHelper.FindDirectoryUpwards("worker-solutions"), $"{problemId}-grid-{rows}-{cols}-{w.orientation}.txt"), res.Moves.StrJoin("\n"));

                    try
                    {
                        SolutionRepo.Submit(
                            new ContestSolution(
                                problemId,
                                score,
                                res.Moves.StrJoin("\n"),
                                new SolverMeta { Description = $"{rows}*{cols} colTolerance={res.ColorTolerance} orientation={w.orientation} prep={w.useSwapperPreprocessor}" },
                                "GridGuidedPainter"));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());
    }
}
