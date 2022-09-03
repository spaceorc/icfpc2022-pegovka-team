using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class SimpleAlgoTests
{
    [Test]
    public void Run([Range(1, 15)] int problemId)
    {
        var problem = Screen.LoadProblem(problemId);
        var algorithm = new SimpleAlgorithm();

        var moves = algorithm.Solve(problem);
        if (moves.Count <= 1)
        {
            return;
        }

        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine(canvas.GetScore(problem));
        canvas.ToScreen().ToImage("res.png");
    }
}
