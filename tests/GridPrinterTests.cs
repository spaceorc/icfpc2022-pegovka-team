using System;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class GridPrinterTests
{
    [Test]
    public async Task Run([Range(1, 25)] int problemId)
    {
        var solver = new GridPrinterAlgorithm(35);
        var screen = Screen.LoadProblem(problemId);
        var (moves, score) = solver.GetBestResult(screen);
        Console.WriteLine($"Score: {score}");
        var solution = moves.StrJoin("\n");
        ClipboardService.SetText(solution);
        Console.WriteLine(solution);
        await SolutionRepo.Submit(new ContestSolution(problemId, (int)score, solution, new SolverMeta(), DateTime.Now, nameof(GridPrinterAlgorithm)));
    }
}
