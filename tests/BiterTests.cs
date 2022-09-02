using System;
using System.Linq;
using System.Text;
using lib;
using lib.Algorithms;
using lib.api;
using NUnit.Framework;

namespace tests;

public class BiterTests
{
    [Test]
    public void RunAll()
    {
        var score = 0;
        for (int i = 1; i <= 15; i++)
        {
            score+=Run(i);
        }

        Console.WriteLine($"TotalScore = {score}");
    }

    [TestCase(1, ExpectedResult = 1)]
    public int Run(int problemNumber)
    {
        var problem = Screen.LoadProblem(problemNumber);
        var moves = new HRibbonBiter().Solve(problem).ToList();
        moves.RemoveAt(moves.Count-1);
        var canvas = new Canvas(problem);
        var res = new StringBuilder();
        foreach (var move in moves)
        {
            //Console.WriteLine(move);
            canvas.Apply(move);
            res.AppendLine(move.ToString());
        }

        //var response = new Api().PostSolution(problemNumber, res.ToString());
        //Console.WriteLine(response?.Submission_Id);
        var score = canvas.GetScore(problem);
        //Console.WriteLine(score);
        canvas.ToScreen().ToImage($"res{problemNumber}.png");
        return score;
    }
}
