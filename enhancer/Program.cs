using lib;
using lib.db;
using lib.Enhancers;

namespace enhancer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                var scoreByProblemId = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
                var combinedEnhancer = new CombinedEnhancer();
                foreach (var (problemId, score) in scoreByProblemId)
                {
                    var solution = SolutionRepo.GetSolutionByIdAndScore(problemId, score).GetAwaiter().GetResult();
                    if (!solution.SolverId.EndsWith("-enchanced"))
                    {
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} not enhanced");
                        var screen = ScreenRepo.GetProblem((int)solution.ProblemId);
                        var eSolution = new ContestSolution(
                            solution.ProblemId,
                            screen.CalculateScore(combinedEnhancer.Enhance(screen, Moves.Parse(solution.Solution))),
                            combinedEnhancer.Enhance(screen, Moves.Parse(solution.Solution)).StrJoin("\n"),
                            new SolverMeta(solution.ScoreEstimated, solution.SolverId),
                            DateTime.UtcNow,
                            solution.SolverId+"-enchanced");
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} enhanced from score {solution.ScoreEstimated} to {eSolution.ScoreEstimated}");
                        SolutionRepo.Submit(eSolution).GetAwaiter().GetResult();
                    }
                }

                Console.WriteLine("sleeping");
                Thread.Sleep(60_000);
            }
        }
    }
}
