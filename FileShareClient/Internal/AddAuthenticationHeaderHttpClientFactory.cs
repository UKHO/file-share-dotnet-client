using System.Net.Http;
using System.Net.Http.Headers;

namespace UKHO.FileShareClient.Internal
{
    internal class AddAuthenticationHeaderHttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactoryImplementation;
        private readonly AuthenticationHeaderValue authenticationHeaderValue;

        public AddAuthenticationHeaderHttpClientFactory(IHttpClientFactory httpClientFactoryImplementation,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            this.httpClientFactoryImplementation = httpClientFactoryImplementation;
            this.authenticationHeaderValue = authenticationHeaderValue;
        }

        public HttpClient CreateClient(string name)
        {
            var httpClient = httpClientFactoryImplementation.CreateClient(name);
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            return httpClient;
        }
    }
}