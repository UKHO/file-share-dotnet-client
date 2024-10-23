using System.Net.Http;

namespace FileShareClientIntegrationTests.Helpers
{
    public class FileShareApiClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new HttpClient();
    }
}
