using System;
using System.Net.Http;

namespace UKHO.FileShareClient.Internal
{
    internal class SetBaseAddressHttpClientFactory : IHttpClientFactory
    {
        private readonly IHttpClientFactory httpClientFactoryImplementation;
        private readonly Uri baseAddress;

        public SetBaseAddressHttpClientFactory(IHttpClientFactory httpClientFactoryImplementation, Uri baseAddress)
        {
            this.httpClientFactoryImplementation = httpClientFactoryImplementation;
            this.baseAddress = baseAddress;
        }

        public HttpClient CreateClient(string name)
        {
            var httpClient = httpClientFactoryImplementation.CreateClient(name);
            httpClient.BaseAddress = baseAddress;
            return httpClient;
        }
    }
}