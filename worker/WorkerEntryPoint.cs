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
    // public static async Task Main_load()
    // {
    //     var path = FileHelper.FindDirectoryUpwards("solutions-with-meta");
    //     var prIds = ScreenRepo.GetProblemIds();
    //     foreach (var problemId in new[] { 30 })
    //     {
    //         Console.Write($"Problem: {problemId}...");
    //
    //         Directory.CreateDirectory(Path.Combine(path, $"problem{problemId}"));
    //
    //         var sols = await SolutionRepo.GetSolutionsByProblemId(problemId);
    //
    //         foreach (var sol in sols)
    //         {
    //             File.WriteAllText(Path.Combine(path, $"problem{problemId}", $"meta-{sol.ScoreEstimated}-{sol.SolverId}.txt"), sol.SolverMeta.ToJson());
    //             File.WriteAllText(Path.Combine(path, $"problem{problemId}", $"sol-{sol.ScoreEstimated}-{sol.SolverId}.txt"), sol.Solution);
    //         }
    //
    //         Console.WriteLine($"{sols.Count} solutions");
    //
    //         await Task.Delay(TimeSpan.FromSeconds(10));
    //     }
    // }

    public static void Main()
    {
        /*
         * 19*19 - 7,10,12,38
         * 17*17 - 9,11,37
         * 15*15 - 18
         * 13*13 - 17,36,39
         *
         */
        var args = new[]
        {
            (19, 19),
            (17, 17),
            (15, 15),
            (13, 13),
        };

        var works = Enumerable.Range(1, 40)
            .SelectMany(problemId => args.Select(a => new { problemId, rows = a.Item1, cols = a.Item2 }))
            .SelectMany(a => Enumerable.Range(0, 8).Select(o => new { a.problemId, a.rows, a.cols, orientation = o }))
            .SelectMany(a => new[] { 0, 4, 8, 10 }.Select(o => new { a.problemId, a.rows, a.cols, a.orientation, swapperPreprocessorN = o }))
            .Where(x =>
            {
                if (new[] { 7, 10, 12, 38 }.Contains(x.problemId))
                    return x.rows == 19;

                if (new[] { 9, 11, 37 }.Contains(x.problemId))
                    return x.rows == 17;

                if (new[] { 18 }.Contains(x.problemId))
                    return x.rows == 15;

                if (new[] { 17, 36, 39 }.Contains(x.problemId))
                    return x.rows == 13;

                return false;
            })
            // .Where(x => x.orientation is 0 or 1)
            .ToArray();

        Console.WriteLine($"Total: {works.Length}");
        var current = 0;

        var tasks = new List<Task>();
        for (int i = 0; i < 96; i++)
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

                    var res = GridGuidedPainterRunner.Solve(problemId, rows, cols, w.orientation, w.swapperPreprocessorN);
                    const string solverId = "GridGuidedPainter";
                    var score = res.Score;

                    long prevBestScore = -1;
                    try
                    {
                        prevBestScore = SolutionRepo.GetBestSolutionBySolverId(problemId, solverId).GetAwaiter().GetResult()!.ScoreEstimated;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    if (score < prevBestScore)
                        Console.WriteLine($"BEST! {localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} o={w.orientation} prep={w.swapperPreprocessorN}");
                    else
                        Console.WriteLine($"{localCurrent + 1}/{works.Length} submitted score={prevBestScore}->{score} problem={problemId} rows={rows} cols={cols} o={w.orientation} prep={w.swapperPreprocessorN}");


                    // File.WriteAllText(Path.Combine(FileHelper.FindDirectoryUpwards("worker-solutions"), $"{problemId}-grid-{rows}-{cols}-{w.orientation}.txt"), res.Moves.StrJoin("\n"));

                    // if (score < prevBestScore)
                        try
                        {
                            SolutionRepo.Submit(
                                new ContestSolution(
                                    problemId,
                                    score,
                                    res.Moves.StrJoin("\n"),
                                    new SolverMeta { Description = $"{rows}*{cols} colTolerance={res.ColorTolerance} orientation={w.orientation} prep={w.swapperPreprocessorN}" },
                                    solverId));
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
