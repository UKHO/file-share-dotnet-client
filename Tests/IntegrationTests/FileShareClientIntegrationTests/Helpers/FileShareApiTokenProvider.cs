using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using UKHO.FileShareClient;

namespace FileShareClientIntegrationTests.Helpers
{
    public class FileShareApiTokenProvider : IAuthTokenProvider
    {
        private readonly string _accessToken;

        public FileShareApiTokenProvider(string clientId, string clientSecret, string fssClientId, string microsoftOnlineLoginUrl, string tenantId)
        {
            _accessToken = CreateToken(clientId, clientSecret, fssClientId, microsoftOnlineLoginUrl, tenantId).Result;
        }

        private static async Task<string> CreateToken(string clientId, string clientSecret, string fssClientId, string microsoftOnlineLoginUrl, string tenantId)
        {
            var scopes = new string[] { $"{fssClientId}/.default" };
            var app = ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(clientSecret).WithAuthority(new Uri($"{microsoftOnlineLoginUrl}{tenantId}")).Build();
            var tokenTask = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            return tokenTask.AccessToken;
        }

        public Task<string> GetToken() => throw new NotImplementedException("Obsolete");

        public async Task<string> GetTokenAsync() => await Task.FromResult(_accessToken);
    }
}
