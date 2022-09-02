using System;
using System.Linq;
using lib;
using lib.Algorithms;
using lib.db;
using NUnit.Framework;

namespace tests;

public class SolutionTests
{
    [Test]
    public void SubmitProblem()
    {
        try
        {
            for (var problemId = 1; problemId <= 1; problemId++)
            {
                var screen = Screen.LoadProblem(1);
                var algorithm = new SimpleAlgorithm();

                var (moves, score) = algorithm.GetBestResult(screen);

                var commands = string.Join('\n', moves.Select(m => m.ToString()));

                var solution = new ContestSolution(problemId, score,
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
}
