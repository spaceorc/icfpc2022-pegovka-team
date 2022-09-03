using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using lib;
using lib.Algorithms;
using lib.api;
using lib.db;
using NUnit.Framework;

namespace tests;

public class SolutionRepoTests
{
    [Test]
    public void SubmitProblemTest([Range(1, 25)] int problemId)
    {
        try
        {
            var screen = Screen.LoadProblem(problemId);
            var algorithm = new SimpleAlgorithm();

            var (moves, score) = algorithm.Solve(screen);

            var commands = string.Join('\n', moves.Select(m => m.ToString()));

            var solution = new ContestSolution(problemId, (long) score,
                commands, new SolverMeta(), DateTime.UtcNow, nameof(SimpleAlgorithm));
            SolutionRepo.Submit(solution).GetAwaiter().GetResult();
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
            if (!nameParts[2].Contains("problem"))
                continue;
            var problemId = int.Parse(nameParts[3]);
            var program = File.ReadAllText(filename);
            var moves = Moves.Parse(program);
            var screen = Screen.LoadProblem(problemId);
            var canvas = new Canvas(screen);
            foreach (var move in moves)
            {
                canvas.Apply(move);
            }

            // api.PostSolution(int.Parse(nameParts[3]), File.ReadAllText(filename));
            var score = canvas.GetScore(screen);
            SolutionRepo.Submit(new ContestSolution(problemId, score, program, new SolverMeta(), DateTime.UtcNow, "manual")).GetAwaiter().GetResult();
        }
        var scoresById = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
        foreach (var (problemId, score) in scoresById)
        {
            var solution = SolutionRepo.GetSolutionByIdAndScore(problemId, score).GetAwaiter().GetResult();
            Console.WriteLine(solution);
        }
    }

    [Test]
    public void METHOD()
    {
        var problemId = 3;
        var solvers = SolutionRepo.GetAllSolvers(problemId).GetAwaiter().GetResult();
        foreach (var solver in solvers)
        {
            if (solver.EndsWith("-enchanced"))
                continue;
            var sol = SolutionRepo.GetBestSolutionBySolverId(problemId, solver).GetAwaiter().GetResult();
            Console.WriteLine($"{sol.SolverId} - {sol.ScoreEstimated}");
        }
    }

    [Test]
    public void SaveBestPngs()
    {
        var path = "..\\..\\..\\..\\best-solutions";
        var prIds = ScreenRepo.GetProblemIds();
        foreach(var problemId in prIds)
        {
            var solvers = SolutionRepo.GetAllSolvers(problemId).GetAwaiter().GetResult();
            foreach (var solver in solvers)
            {
                var sol = SolutionRepo.GetBestSolutionBySolverId(problemId, solver).GetAwaiter().GetResult();
                Console.WriteLine($"{sol.SolverId} - {sol.ScoreEstimated}");
                var screen = ScreenRepo.GetProblem(problemId);
                var canvas = new Canvas(screen);
                var moves = Moves.Parse(sol.Solution);
                foreach (var move in moves)
                {
                    canvas.Apply(move);
                }

                var finalCanvas = canvas.ToScreen();
                finalCanvas.ToImage(Path.Combine(path, $"sol-{problemId}-{sol.SolverId}.png"));
            }
        }
    }
}
