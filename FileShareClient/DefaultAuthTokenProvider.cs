using System.Threading.Tasks;

namespace UKHO.FileShareClient
{
    public interface IAuthTokenProvider
    {
        Task<string> GetToken();
    }

    internal class DefaultAuthTokenProvider: IAuthTokenProvider
    {
        private readonly string accessToken;

        public DefaultAuthTokenProvider(string accessToken)
        {
            this.accessToken = accessToken;
        }

        public Task<string> GetToken()
        {
            return Task.FromResult(accessToken);
        }
    }
}
