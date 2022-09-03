using lib;
using lib.api;
using lib.db;
using Spectre.Console;

namespace submitter
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var api = new Api();
            while (true)
            {
                var submissionsInfos = api.GetSubmissionsInfo();
                var problemIdToInfo = submissionsInfos!.Submissions.ToDictionary(s => s.Id, s => s);
                var scoreByProblemId = SolutionRepo.GetBestScoreByProblemId().GetAwaiter().GetResult();
                Parallel.ForEach(scoreByProblemId, problemIdAndScore =>
                    //foreach (var (problemId, score) in scoreByProblemId)
                {
                    var (problemId, score) = problemIdAndScore;
                    var solution = SolutionRepo.GetSolutionByProblemIdAndScore(problemId, score).GetAwaiter().GetResult();
                    if (solution.SubmissionId == null)
                    {
                        var submissionResult = api.PostSolution(solution.ProblemId, solution.Solution);
                        solution.SubmittedAt = DateTime.UtcNow;
                        solution.SubmissionId = submissionResult?.Submission_Id;
                        Console.WriteLine($"Submit solution for problem {problemId} with score {score} by {solution.SolverId}");
                        SolutionRepo.Submit(solution);
                        //continue;
                    } else if (solution.ScoreServer == null && problemIdToInfo.ContainsKey((long)solution.SubmissionId))
                    {
                        solution.ScoreServer = problemIdToInfo[solution.SubmissionId.Value].Score;
                        Console.WriteLine($"Add ScoreServer for problem {problemId} - score: {score}, serverScore: {solution.ScoreServer}, submissionId {solution.SubmissionId}");
                        SolutionRepo.Submit(solution);
                    }
                });
                Thread.Sleep(60_000);
            }
        }

        private static void RefreshDashboard(List<string> logMessages, Api api, SubmissionRepo submissionRepo)
        {
            // var worldBestDislikes = new ProblemTableApi().FetchDislikesAsync().Result.ToDictionary(t => t.ProblemId);

            var problemIds = ScreenRepo.GetProblemIds();
            var submitterRepo = new SubmissionRepo(new Settings());

            foreach (var id in problemIds)
            {

                var problem = ScreenRepo.GetProblem(id);
                // var state = new State();

                var result = api.PostSolution(id, " ");
                // submissionRepo.PutFile()
                Console.WriteLine(result);

            }

            Console.Clear();
            AnsiConsole.Write("");
        }
    }
}
