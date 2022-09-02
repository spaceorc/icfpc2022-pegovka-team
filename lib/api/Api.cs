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

        private const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InhvcG9zaGl5QGJrLnJ1IiwiZXhwIjoxNjYyMjA2NTQ2LCJvcmlnX2lhdCI6MTY2MjEyMDE0Nn0.f_zL83gcXhtupbWTH14LA1ihRSuLtAkGjekT-oY03go";

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

        public SubmissionResult? PostSolution(int problemId, string text)
        {

            using var content = new MultipartFormDataContent("------WebKitFormBoundaryLBbYAgAJs3gT1Isi");
            content.Add(new StreamContent(new MemoryStream(Encoding.ASCII.GetBytes(text))),
                "file", "submission.isl");

            var response = Client.PostAsync($"https://robovinci.xyz/api/submissions/{problemId}/create", content).GetAwaiter().GetResult();

            return response.Content.ReadFromJsonAsync<SubmissionResult>().GetAwaiter().GetResult();
        }

        public record SubmissionResult(int Submission_Id);

        // public record SubmissionResult(int Id, string ProblemId, int Score, string Status, DateTime SubmittedAt);

        public record ProblemsInfo(ProblemInfo[] Problems)
        {
            public override string ToString()
            {
                return $"{nameof(Problems)}: {string.Join(" ", Problems.ToList())}";
            }
        }

        public record ProblemInfo(int Id, string Name, string Description, string Canvas_Link, string Target_Link, string Initial_Config_File);
    }
}
