using System;
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
    public async Task Run([Range(1, 1)] int problemId)
    {
        //await SolutionRepo.Submit(new ContestSolution(problemId, (int)29856, File.ReadAllText(@"c:\work\contests\icfpc-2022\hand-solutions\problem-1-grid.txt "), new SolverMeta(), DateTime.Now, nameof(GridPrinterAlgorithm)));
        var bestScore = double.PositiveInfinity;
        foreach (var cellSize in new[]{40})
        {
            var solver = new LayersAlgorithm(cellSize);
            var screen = Screen.LoadProblem(problemId).MedianFilter();
            screen.ToImage("filtered.png");
            var (moves, score) = solver.GetBestResult(screen);

            Console.WriteLine($"Score: {score} cellSize: {cellSize}");
            var solution = moves.StrJoin("\n");
            if (score < bestScore)
            {
                bestScore = score;
                await ClipboardService.SetTextAsync(solution);
                await SolutionRepo.SubmitAsync(new ContestSolution(problemId, (int)score, solution, new SolverMeta(), nameof(LayersAlgorithm)+cellSize));
            }
        }
    }
}
