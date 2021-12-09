using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public class Result<T> : IResult<T>
    {
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public async Task<T> GetResponse(HttpResponseMessage response)
        {
            return await response.ReadAsTypeAsync<T>();
        }
    }
}
