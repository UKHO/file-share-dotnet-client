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

        public T Data { get; set; }

        //public async Task<T> GetResponseData(HttpResponseMessage response)
        //{
        //    return await response.ReadAsTypeAsync<T>();
        //}

        public async Task<T> GetResponseData1(HttpResponseMessage response)
        {
            return await response.ReadAsTypeAsync<T>();
        }

        public async Task<IResult<T>> GetResponseData(HttpResponseMessage response)
        {
            //return await response.ReadAsTypeAsync<Result<T>>();

            var data = await response.ReadAsTypeAsync<Result<T>>();

            data = data ?? new Result<T>();

            data.IsSuccess = response.IsSuccessStatusCode;
            data.StatusCode = (int)response.StatusCode;

            return data;
        }
    }
}
