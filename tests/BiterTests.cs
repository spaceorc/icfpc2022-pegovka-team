using System;
using System.Linq;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class BiterTests
{
    [Test]
    public void RunAll()
    {
        for (int i = 1; i <= 15; i++)
        {
            Run(i);
        }
    }

    [TestCase(1)]
    public void Run(int problemNumber)
    {
        var problem = Screen.LoadProblem(problemNumber);
        var moves = new HRibbonBiter().Solve(problem).ToList();
        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage($"res{problemNumber}.png");

    }
}
