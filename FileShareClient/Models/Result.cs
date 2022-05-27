using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;

namespace UKHO.FileShareClient.Models
{
    public class Result<T> : IResult<T>
    {
        public bool IsSuccess { get; set; }

        public int StatusCode { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public T Data { get; set; }
    }

    internal static class Result
    {
        internal static async Task<IResult<Stream>> WithStreamData(HttpResponseMessage response)
        {
            Stream data = default;

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                data = await response.ReadAsStreamAsync();
            }

            return await CreateResult(response, data);
        }

        internal static async Task<IResult<U>> WithObjectData<U>(HttpResponseMessage response)
        {
            U data = default;

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                data = await response.ReadAsTypeAsync<U>();
            }

            return await CreateResult(response, data);
        }

        //this is strange and exists to keep backwards compatibility for the FileShareApiClient.DownloadFileAsync method
        internal static async Task<IResult<U>> WithAlwaysDefaultData<U>(HttpResponseMessage response)
        {
            return await CreateResult<U>(response, default);
        }

        private static async Task<IResult<U>> CreateResult<U>(HttpResponseMessage response, U data)
        {
            var result = new Result<U>();
            result.IsSuccess = response.IsSuccessStatusCode;
            result.StatusCode = (int)response.StatusCode;
            result.Data = data;
            result.Errors = new List<Error>();

            if (response.Content != null && !response.IsSuccessStatusCode)
            {
                try
                {
                    var errorResponse = await response.ReadAsTypeAsync<ErrorResponseModel>();
                    result.Errors = errorResponse?.Errors ?? new List<Error>();
                }
                catch
                {
                    var content = await response.ReadAsTypeAsync<string>();
                    result.Errors = new List<Error> { new Error { Description = content } };
                }
            }

            return result;
        }
    }
}
