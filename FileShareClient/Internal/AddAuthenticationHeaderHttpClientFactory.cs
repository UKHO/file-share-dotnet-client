using System.Net.Http;
using System.Net.Http.Headers;

namespace UKHO.FileShareClient.Internal
{
    internal class AddAuthenticationHeaderHttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactoryImplementation;
        private readonly IAuthTokenProvider authTokenProvider;

        public AddAuthenticationHeaderHttpClientFactory(IHttpClientFactory httpClientFactoryImplementation,
            IAuthTokenProvider authTokenProvider)
        {
            this.httpClientFactoryImplementation = httpClientFactoryImplementation;
            this.authTokenProvider = authTokenProvider;
        }

        public HttpClient CreateClient(string name)
        {
            var httpClient = httpClientFactoryImplementation.CreateClient(name);
            //Note: using "Result" of task here in absence of async support - need to revisit
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authTokenProvider.GetToken().Result);
            return httpClient;
        }
    }
}