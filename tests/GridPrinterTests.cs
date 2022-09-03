using System;
using lib;
using lib.Algorithms;
using NUnit.Framework;
using TextCopy;

namespace tests;

public class GridPrinterTests
{
    [Test]
    public void Run([Range(11, 11)] int problemId)
    {
        var solver = new GridPrinterAlgorithm(40);
        var screen = Screen.LoadProblem(problemId);
        var (moves, score) = solver.GetBestResult(screen);
        Console.WriteLine($"Score: {score}");
        var solution = moves.StrJoin("\n");
        ClipboardService.SetText(solution);
        Console.WriteLine(solution);
    }
}
