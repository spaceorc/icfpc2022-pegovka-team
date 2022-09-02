using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public async Task RunGreedyRectBiter([Range(1, 20)]int problemId)
    {
        var solver = new GreedyRectBiter(new Random());
        var problem = Screen.LoadProblem(problemId);
        var canvas = new Canvas(problem);
        var state = new BiterState(canvas, new HashSet<string>(), problem);
        var (moves, score) = solver.Solve(state, 1000);
        var res = moves.StrJoin("\n");
        Console.WriteLine(res);
        await SolutionRepo.Submit(new ContestSolution(problemId, score, res, new SolverMeta(), DateTime.Now, solver.ToString()!));
        //var response = new Api().PostSolution(problemId, res);
        //Console.WriteLine(response?.Submission_Id);

    }
}
