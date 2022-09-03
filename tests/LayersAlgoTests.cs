﻿using System;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class LayersAlgoTests
{
    [Test]
    [Parallelizable(ParallelScope.All)]
    public async Task Run([Range(1, 25)] int problemId)
    {
        //await SolutionRepo.Submit(new ContestSolution(problemId, (int)29856, File.ReadAllText(@"c:\work\contests\icfpc-2022\hand-solutions\problem-1-grid.txt "), new SolverMeta(), DateTime.Now, nameof(GridPrinterAlgorithm)));
        foreach (var cellSize in new[]{20, 40})
        {
            var solver = new LayersAlgorithm(cellSize);
            var screen = Screen.LoadProblem(problemId);
            var (moves, score) = solver.GetBestResult(screen);
            Console.WriteLine($"Score: {score} cellSize: {cellSize}");
            var solution = moves.StrJoin("\n");
            await ClipboardService.SetTextAsync(solution);
            await SolutionRepo.SubmitAsync(new ContestSolution(problemId, (int)score, solution, new SolverMeta(), nameof(LayersAlgorithm)+cellSize));
        }
    }
}