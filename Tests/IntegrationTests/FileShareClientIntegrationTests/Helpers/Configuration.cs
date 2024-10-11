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

            var fssAuthSection = configurationRoot!.GetSection("FssAuth") ?? throw new NullReferenceException("Unable to find FssAuth section in appsettings.json");
            var clientId = GetFssAuthValue("ClientId");
            var clientSecret = GetFssAuthValue("ClientSecret");
            var fssClientId = GetFssAuthValue("FssClientId");
            var microsoftOnlineLoginUrl = GetFssAuthValue("MicrosoftOnlineLoginUrl");
            var tenantId = GetFssAuthValue("TenantId");
            AuthTokenProvider = new FileShareApiTokenProvider(clientId, clientSecret, fssClientId, microsoftOnlineLoginUrl, tenantId);

            FssUrl = configurationRoot.GetValue<string>("FssUrl") ?? throw new NullReferenceException("Unable to find FssUrl value in appsettings.json");

            GetBatchStatusAsync = new GetBatchStatusAsyncModel { BatchId = configurationRoot.GetValue<string>("GetBatchStatusAsync:BatchId") };

            string GetFssAuthValue(string key) => fssAuthSection!.GetValue<string>(key) ?? throw new NullReferenceException($"Unable to find {key} in FssAuth section in appsettings.json");
        }
    }
}
