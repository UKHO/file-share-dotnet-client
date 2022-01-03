using System.Collections.Generic;
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

        /// <summary>
        /// Deserialize the response data.
        /// </summary>
        /// <param name="successCode">Expected HttpStatusCode</param>
        /// <param name="response">API response</param>
        /// <returns></returns>
        internal async Task ProcessHttpResponse(HttpStatusCode successCode, HttpResponseMessage response)
        {
            IsSuccess = response.IsSuccessStatusCode;
            StatusCode = (int)response.StatusCode;

            if (response.Content != null)
            {
                if (response.StatusCode.CompareTo(successCode) == 0)
                {
                    Data = await response.ReadAsTypeAsync<T>();
                }
                else
                {
                    var errorResponse = await response.ReadAsTypeAsync<ErrorResponseModel>();

                    Errors = errorResponse == null ? null : errorResponse.Errors;
                }
            }
        }
    }
}
