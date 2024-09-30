using System;
using System.Threading.Tasks;

namespace UKHO.FileShareClient
{
    public interface IAuthTokenProvider
    {
        Task<string> GetTokenAsync();

        #region backwards compatible obsolete versions of methods that have been renamed.

        [Obsolete("please use GetTokenAsync")]
        Task<string> GetToken();

        #endregion
    }

    internal class DefaultAuthTokenProvider : IAuthTokenProvider
    {
        private readonly string _accessToken;

        public DefaultAuthTokenProvider(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<string> GetTokenAsync()
        {
            return Task.FromResult(_accessToken);
        }

        #region backwards compatible obsolete versions of methods that have been renamed.

        [Obsolete("please use GetTokenAsync")]
        public Task<string> GetToken()
        {
            return GetTokenAsync();
        }

        #endregion
    }
}