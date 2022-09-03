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
                var scoreByProblemId = SolutionRepo.GetBestScoreByProblemIdAndSolverId().GetAwaiter().GetResult();
                Parallel.ForEach(scoreByProblemId, parms =>
                    //foreach (var (problemId, solverId, score) in scoreByProblemId)
                {
                    var (problemId, solverId, score) = parms;
                    var solution = SolutionRepo.GetSolutionByProblemIdAndSolverIdAndScore(problemId, solverId, score).GetAwaiter().GetResult();
                    if (!solution.SolverId.EndsWith("-enchanced") && solution.SolverMeta.Enhancer_Id == null)
                    {
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} not enhanced");
                        var screen = ScreenRepo.GetProblem((int)solution.ProblemId);
                        var eMoves = Enhancer.Enhance(screen, Moves.Parse(solution.Solution));
                        var eSolution = new ContestSolution(
                            solution.ProblemId,
                            screen.CalculateScore(eMoves),
                            eMoves.StrJoin("\n"),
                            new SolverMeta(solution.ScoreEstimated, solution.SolverId),
                            solution.SolverId + "-enchanced");
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} enhanced from score {solution.ScoreEstimated} to {eSolution.ScoreEstimated}");
                        SolutionRepo.Submit(eSolution);

                        solution.SolverMeta.Enhancer_Id = "default";
                        SolutionRepo.Submit(solution);
                    }
                });

                Console.WriteLine("sleeping");
                Thread.Sleep(60_000);
            }
        }
    }
}
