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

                Console.WriteLine("getting solutions from DB");
                var solutions = scoreByProblemId.ToDictionary(
                    e => (e.problemId, e.solverId),
                    e => SolutionRepo.GetSolutionByProblemIdAndSolverIdAndScore(e.problemId, e.solverId, e.score).GetAwaiter().GetResult());
                Console.WriteLine("got solutions from DB");

                Parallel.ForEach(scoreByProblemId, parms =>
                {
                    var (problemId, solverId, score) = parms;
                    var solution = solutions[(problemId, solverId)];
                    if (!solution.SolverId.EndsWith("-enchanced") && solution.SolverMeta.Enhancer_Id != "enchancer2")
                    {
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} not enhanced2");
                        var screen = ScreenRepo.GetProblem((int)solution.ProblemId);
                        var eMoves = Enhancer.Enhance2(screen, Moves.Parse(solution.Solution));
                        var eSolution = new ContestSolution(
                            solution.ProblemId,
                            screen.CalculateScore(eMoves),
                            eMoves.StrJoin("\n"),
                            new SolverMeta(solution.ScoreEstimated, solution.SolverId),
                            solution.SolverId + "-2-enchanced");
                        Console.WriteLine($"solution {solution.SolverId} for problem {solution.ProblemId} enhanced2 from score {solution.ScoreEstimated} to {eSolution.ScoreEstimated}");
                        SolutionRepo.Submit(eSolution);

                        solution.SolverMeta.Enhancer_Id = "enchancer2";
                        SolutionRepo.Submit(solution);
                    }
                });

                Console.WriteLine("sleeping");
                Thread.Sleep(60_000);
            }
        }
    }
}
