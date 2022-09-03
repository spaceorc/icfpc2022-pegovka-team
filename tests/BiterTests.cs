using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using lib;
using lib.Algorithms;
using lib.Algorithms.RectBiter;
using lib.api;
using lib.db;
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
    public int Run(int problemId)
    {
        var problem = Screen.LoadProblem(problemId);
        var moves = new HRibbonBiter().Solve(problem).ToList();
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
        canvas.ToScreen().ToImage($"res{problemId}.png");
        return score;
    }

    [Test]
    [Parallelizable(ParallelScope.All)]
    public async Task RunGreedyRectBiter([Range(9, 9)]int problemId)
    {
        var solver = new GreedyRectBiter(new Random(123123));
        var problem = Screen.LoadProblem(problemId);
        var minScore = long.MaxValue;
        var bestRes = "";
        for (int i = 0; i < 5; i++)
        {
            var canvas = new Canvas(problem);
            var state = new BiterState(canvas, new HashSet<string>(), problem);
            var (moves, score) = solver.Solve(state, 100);
            if (score < minScore)
            {
                minScore = score;
                bestRes = moves.StrJoin("\n");
                Console.WriteLine(minScore + " " + DateTime.Now);
            }
        }
        await SolutionRepo.SubmitAsync(new ContestSolution(problemId, minScore, bestRes, new SolverMeta(), solver.ToString()!));
        Console.WriteLine(bestRes);
        //var response = new Api().PostSolution(problemId, res);
        //Console.WriteLine(response?.Submission_Id);
    }

    [Test]
    public void X()
    {
        var problem = Screen.LoadProblem(3);
        var canvas = new Canvas(problem);
        var block = canvas.Blocks.Values.Single();
        var color = problem.GetAverageColorByGeometricMedian(block);
        canvas.ApplyColor(new ColorMove(block.Id, color));
        Console.WriteLine(canvas.GetScore(problem));
        //19
        //78270
        //89879
        //72227
        //21
        //112375
        //111479
        //110952
        //3
        //91814
        //128010
        //65067

    }
}
