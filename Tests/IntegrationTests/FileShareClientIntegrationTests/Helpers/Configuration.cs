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
        public static SearchAsyncModel SearchAsync { get; }
        public static DownloadFileAsyncModel DownloadFileAsync { get; }

        static Configuration()
        {
            var configurationRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build() ?? throw new NullReferenceException("Unable to build configuration from appsettings.json");

            HttpClientFactory = new FileShareApiClientFactory();

            var clientId = GetString("FssAuth:ClientId");
            var clientSecret = GetString("FssAuth:ClientSecret");
            var fssClientId = GetString("FssAuth:FssClientId");
            var microsoftOnlineLoginUrl = GetString("FssAuth:MicrosoftOnlineLoginUrl");
            var tenantId = GetString("FssAuth:TenantId");
            AuthTokenProvider = new FileShareApiTokenProvider(clientId, clientSecret, fssClientId, microsoftOnlineLoginUrl, tenantId);

            FssUrl = GetString("FssUrl");

            GetBatchStatusAsync = new GetBatchStatusAsyncModel { BatchId = GetString("GetBatchStatusAsync:BatchId") };

            SearchAsync = new SearchAsyncModel
            {
                SearchQuery = GetString("SearchAsync:SearchQuery"),
                PageSize = GetInt("SearchAsync:PageSize"),
                Start = GetInt("SearchAsync:Start")
            };

            DownloadFileAsync = new DownloadFileAsyncModel
            {
                BatchId = GetString("DownloadFileAsync:BatchId"),
                FileName = GetString("DownloadFileAsync:FileName")
            };

            string GetString(string key) => configurationRoot!.GetValue<string>(key) ?? throw new NullReferenceException($"Unable to find {key} in appsettings.json");
            int GetInt(string key) => configurationRoot!.GetValue<int>(key);
        }
    }
}
