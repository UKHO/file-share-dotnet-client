using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public class Result<T> : IResult<T>
    {
        private readonly HttpResponseMessage response;
        public Result(HttpResponseMessage response)
        {
            this.response = response;
            IsSuccess = response.IsSuccessStatusCode;
            StatusCode = (int)response.StatusCode;
        }
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public T Property { get; set; }

        public async Task<T> GetResponseData()
        {
            return await response.ReadAsTypeAsync<T>();
        }
    }
}
