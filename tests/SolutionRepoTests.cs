using System;
using System.IO;
using System.Linq;
using lib;
using lib.Algorithms;
using lib.api;
using lib.db;
using NUnit.Framework;

namespace tests;

public class SolutionRepoTests
{
    [Test]
    public void SubmitProblemTest()
    {
        try
        {
            for (var problemId = 1; problemId <= 15; problemId++)
            {
                var screen = Screen.LoadProblem(problemId);
                var algorithm = new SimpleAlgorithm();

                var (moves, score) = algorithm.GetBestResult(screen);

                var commands = string.Join('\n', moves.Select(m => m.ToString()));

                var solution = new ContestSolution(problemId, (long) score,
                    commands, new SolverMeta(), DateTime.UtcNow, nameof(SimpleAlgorithm));
                SolutionRepo.Submit(solution).GetAwaiter().GetResult();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    [Test]
    public void GetBestScoreByProblemIdTest()
    {
        var ans = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        Console.WriteLine(string.Join(" ", ans));
    }

    [Test]
    public void GetSolutionByIdAndScoreTest()
    {
        var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        foreach (var (problemId, score) in scoresById)
        {
            var solution = SolutionRepo.GetSolutionByIdAndScore(problemId, score).GetAwaiter().GetResult();
            Console.WriteLine(solution);
        }
    }

    [Test]
    public void SubmitManualSolutions()
    {
        var api = new Api();
        var handsDirectory = FileHelper.FindDirectoryUpwards("hand-solutions");
        var filenames = Directory.GetFiles(handsDirectory, "*.txt");
        foreach (var filename in filenames)
        {
            var nameParts = filename.Split('-');
            if (nameParts[0] != "problem")
                continue;
            var problemId = int.Parse(nameParts[1]);
            var program = File.ReadAllText(filename);
            var moves = Moves.Parse(program);
            var screen = Screen.LoadProblem(problemId);
            var canvas = new Canvas(screen);
            foreach (var move in moves)
            {
                canvas.Apply(move);
            }

            var score = canvas.GetScore(screen);
            SolutionRepo.Submit(new ContestSolution(problemId, score, program, new SolverMeta(), DateTime.UtcNow, "manual"));
            // api.PostSolution(int.Parse(nameParts[1]), File.ReadAllText(filename));
        }
        var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        foreach (var (problemId, score) in scoresById)
        {
            var solution = SolutionRepo.GetSolutionByIdAndScore(problemId, score).GetAwaiter().GetResult();
            Console.WriteLine(solution);
        }
    }
}
