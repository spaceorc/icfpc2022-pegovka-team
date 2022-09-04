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
            (5, 40),
            (7, 40),
            (9, 40),
            (11, 40),
            (13, 40),
            (15, 40)
        };

        var works = Enumerable.Range(1, 40)
            .SelectMany(problemId => args.Select(a => new { problemId, rows = a.Item1, cols = a.Item2 }))
            .SelectMany(a => Enumerable.Range(0, 8).Select(o => new { a.problemId, a.rows, a.cols, orientation = o }))
            .Where(x => x.problemId <= 25)
            .Where(x => x.orientation == 0)
            .ToArray();

        Console.WriteLine($"Total: {works.Length}");
        var current = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 2; i++)
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

                    var res = GridGuidedPainterRunner.Solve(problemId, rows, cols, w.orientation);
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
                        Console.WriteLine($"BEST! {localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} orientation={w.orientation}");
                    else
                        Console.WriteLine($"{localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} orientation={w.orientation}");


                    // File.WriteAllText(Path.Combine(FileHelper.FindDirectoryUpwards("worker-solutions"), $"{problemId}-grid-{rows}-{cols}-{w.orientation}.txt"), res.Moves.StrJoin("\n"));

                    try
                    {
                        SolutionRepo.Submit(
                            new ContestSolution(
                                problemId,
                                score,
                                res.Moves.StrJoin("\n"),
                                new SolverMeta { Description = $"{rows}*{cols} colTolerance={res.ColorTolerance} orientation={w.orientation}" },
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
