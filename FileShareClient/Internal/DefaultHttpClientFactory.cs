using System.Net.Http;

namespace UKHO.FileShareClient.Internal
{
    internal class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}