using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace WorkerService
{
    internal static class PathHelper
    {
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        public static async Task<string> GetJsonResponse(string endPoint)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            HttpClient httpClient = new HttpClient(handler);

            HttpResponseMessage cpuResponse = await httpClient.GetAsync(endPoint);
            cpuResponse.EnsureSuccessStatusCode();
            string responseBody = await cpuResponse.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}