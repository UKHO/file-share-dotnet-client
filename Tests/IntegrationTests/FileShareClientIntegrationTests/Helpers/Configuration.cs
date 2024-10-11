using FileShareClientIntegrationTests.Models;
using Microsoft.Extensions.Configuration;
using UKHO.FileShareClient;

namespace FileShareClientIntegrationTests.Helpers
{
    public static class Configuration
    {
        public static IHttpClientFactory HttpClientFactory { get; }
        public static IAuthTokenProvider AuthTokenProvider { get; }
        public static string FssUrl { get; }
        public static GetBatchStatusAsyncModel GetBatchStatusAsync { get; }

        static Configuration()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build() ?? throw new NullReferenceException("Unable to build configuration from appsettings.json");

            HttpClientFactory = new FileShareApiClientFactory();

            var clientId = GetValue("FssAuth:ClientId");
            var clientSecret = GetValue("FssAuth:ClientSecret");
            var fssClientId = GetValue("FssAuth:FssClientId");
            var microsoftOnlineLoginUrl = GetValue("FssAuth:MicrosoftOnlineLoginUrl");
            var tenantId = GetValue("FssAuth:TenantId");
            AuthTokenProvider = new FileShareApiTokenProvider(clientId, clientSecret, fssClientId, microsoftOnlineLoginUrl, tenantId);

            FssUrl = GetValue("FssUrl");

            GetBatchStatusAsync = new GetBatchStatusAsyncModel { BatchId = GetValue("GetBatchStatusAsync:BatchId") };

            string GetValue(string key) => configurationRoot!.GetValue<string>(key) ?? throw new NullReferenceException($"Unable to find {key} in appsettings.json");
        }
    }
}
