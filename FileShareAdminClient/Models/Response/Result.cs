using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public T Data { get; set; }

        public async Task ProcessHttpResponse(HttpStatusCode successCode,  HttpResponseMessage response)
        {
            IsSuccess = response.IsSuccessStatusCode;
            StatusCode = (int)response.StatusCode;

            if(response.Content != null)
            {
                if (response.StatusCode == successCode)
                {
                    Data = await response.ReadAsTypeAsync<T>();
                }
                else
                {
                    var errorResponse = await response.ReadAsTypeAsync<Error[]>();
                    Errors = errorResponse.ToList();
                }
            }
        }
    }
}
