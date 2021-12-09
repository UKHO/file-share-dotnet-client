using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public class Result<T> : IResult<T>
    {
        //private readonly HttpResponseMessage response;

        //public Result(HttpResponseMessage response)
        //{
            
        //    this.response = response;

        //    IsSuccess = response.IsSuccessStatusCode;

        //    StatusCode = (int)response.StatusCode;
        //}
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        //  public T TypeProperties { get; private set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public async Task<T> GetResponse(HttpResponseMessage response)
        {
            //T data = default(T);

            IsSuccess = response.IsSuccessStatusCode;

            StatusCode = (int)response.StatusCode;

            return await response.ReadAsTypeAsync<T>();
        }
    }
}
