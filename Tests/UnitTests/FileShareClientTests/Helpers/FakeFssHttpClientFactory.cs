using System;
using System.IO;
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
            var (httpStatusCode, responseValue) = httpMessageHandler(request);
            var response = new HttpResponseMessage
            {
                StatusCode = httpStatusCode
            };

            switch (responseValue)
            {
                case null:
                    break;
                case Stream stream:
                    response.Content = new StreamContent(stream);
                    break;
                default:
                    response.Content = new StringContent(JsonConvert.SerializeObject(responseValue), Encoding.UTF8,
                        "application/json");
                    break;
            }

            return Task.FromResult(response);
        }
    }
}