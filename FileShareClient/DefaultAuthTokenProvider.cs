using System.Threading.Tasks;

namespace UKHO.FileShareClient
{
    public interface IAuthTokenProvider
    {
        Task<string> GetToken();
    }

    internal class DefaultAuthTokenProvider : IAuthTokenProvider
    {
        private readonly string _accessToken;

        public DefaultAuthTokenProvider(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<string> GetToken()
        {
            return Task.FromResult(_accessToken);
        }
    }
}
