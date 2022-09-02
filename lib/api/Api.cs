using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace lib.api
{
    public class Api
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string host;
        private const string pathToSave = "..\\..\\..\\..\\problems";

        public Api(string host = "https://cdn.robovinci.xyz")
        {
            this.host = host;
        }

        public async Task<bool> DownloadProblem(int problemId)
        {
            var stream = await Client.GetStreamAsync($"{host}/imageframes/{problemId}.png");
            var fileStream = new FileStream($"{pathToSave}\\{problemId}.png", FileMode.OpenOrCreate);
            try
            {
                await stream.CopyToAsync(fileStream);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<byte[]> FetchProblem(int problemId)
        {
            return await Client.GetByteArrayAsync($"{host}/imageframes/{problemId}.png");
        }

        public SubmissionResult? PostSolution(string problemId, byte[] content)
        {
            throw new NotImplementedException();
            // var response = Client.PostAsync($"{host}/problems/{problemId}", content).GetAwaiter().GetResult();
            // return response.Content.ReadFromJsonAsync<SubmissionResult>().GetAwaiter().GetResult();
        }
    }

    public record SubmissionResult(string Id, string Error)
    {
    }
}
