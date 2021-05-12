using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKHO.FileShareClientTests.Helpers
{
    public class FakeFssHttpClientFactory : DelegatingHandler, IHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode, object)> httpMessageHandler;

        public FakeFssHttpClientFactory(Func<HttpRequestMessage, (HttpStatusCode, object)> httpMessageHandler)
        {
            this.httpMessageHandler = httpMessageHandler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(this);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var responseData = httpMessageHandler(request);
            var response = new HttpResponseMessage
            {
                StatusCode = responseData.Item1
            };
            if (responseData.Item2 != null)
                response.Content = new StringContent(JsonConvert.SerializeObject(responseData.Item2), Encoding.UTF8,
                    "application/json");

            return Task.FromResult(response);
        }
    }
}