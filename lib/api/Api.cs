using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace lib.api
{
    public class Api
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string basicHost;
        private readonly string sendingHost;
        private const string pathToSave = "..\\..\\..\\..\\problems";

        private const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InhvcG9zaGl5QGJrLnJ1IiwiZXhwIjoxNjYyMzc1ODA1LCJvcmlnX2lhdCI6MTY2MjI4OTQwNX0.H0aX8HKsOfuuoXcGqAEQp2QwIDTuEd8B6buquejdcXw";

        public Api(string sendingHost = "https://robovinci.xyz", string basicHost = "https://cdn.robovinci.xyz")
        {
            this.basicHost = basicHost;
            this.sendingHost = sendingHost;

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public ProblemsInfo? GetAllProblems()
        {
            var response = Client.GetAsync($"{sendingHost}/api/problems").GetAwaiter().GetResult();
            Console.WriteLine(response);
            return response.Content.ReadFromJsonAsync<ProblemsInfo>().GetAwaiter().GetResult();
        }

        // public async Task<bool> DownloadProblem(int problemId)
        // {
        //     var stream = await Client.GetStreamAsync($"{basicHost}/imageframes/{problemId}.png");
        //     var fileStream = new FileStream($"{pathToSave}\\{problemId}.png", FileMode.OpenOrCreate);
        //     try
        //     {
        //         await stream.CopyToAsync(fileStream);
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return false;
        //     }
        // }

        public async Task<byte[]> FetchProblem(int problemId)
        {
            return await Client.GetByteArrayAsync($"{basicHost}/imageframes/{problemId}.png");
        }

        public FullSubmissionResults? GetSubmissionsInfo()
        {
            var response = Client.GetAsync($"{sendingHost}/api/submissions").GetAwaiter().GetResult();
            var g = response.Content.ReadAsStringAsync();
            return response.Content.ReadFromJsonAsync<FullSubmissionResults>().GetAwaiter().GetResult();
        }


        public SubmissionResult? PostSolution(long problemId, string text)
        {

            using var content = new MultipartFormDataContent("------WebKitFormBoundaryLBbYAgAJs3gT1Isi");
            content.Add(new StreamContent(new MemoryStream(Encoding.ASCII.GetBytes(text))),
                "file", "submission.isl");

            var response = Client.PostAsync($"{sendingHost}/api/submissions/{problemId}/create", content).GetAwaiter().GetResult();

            return response.Content.ReadFromJsonAsync<SubmissionResult>().GetAwaiter().GetResult();
        }

        public record SubmissionResult(int Submission_Id);


        public record ProblemsInfo(ProblemInfo[] Problems)
        {
            public override string ToString()
            {
                return $"{nameof(Problems)}: {string.Join(" ", Problems.ToList())}";
            }
        }

        public record ProblemInfo(int Id, string Name, string Description, string Canvas_Link, string Target_Link, string Initial_Config_File);

        public record FullSubmissionResults(FullSubmissionResult[] Submissions)
        {
            public override string ToString()
            {
                return $"{nameof(Submissions)}: {string.Join(" ", Submissions.ToList())}";
            }
        }

        public record FullSubmissionResult(long Id, long Problem_id, long Score, string Status, DateTime Submitted_at);
    }
}
