using System;
using System.IO;
using System.Linq;
using lib;
using lib.Algorithms;
using NUnit.Framework;

namespace tests;

public class EnchancerTests
{
    [Test]
    public void Run()
    {
        var problem = Screen.LoadProblem(1);
        var moves = Moves.Parse(File.ReadAllText(FileHelper.FindFilenameUpwards("hand-solutions/problem1.txt")));
        var canvas = new Canvas(problem);
        foreach (var move in moves)
        {
            Console.WriteLine(move);
            canvas.Apply(move);
        }

        Console.WriteLine();
        Console.WriteLine(canvas.GetScore(problem));
    }
}
