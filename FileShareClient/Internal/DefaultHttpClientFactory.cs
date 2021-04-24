using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace UKHO.FileShareClient.Internal
{
    [ExcludeFromCodeCoverage]
    internal class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}