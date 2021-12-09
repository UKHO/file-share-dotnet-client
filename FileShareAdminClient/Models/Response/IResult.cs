using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public interface IResult<T>
    {
        bool IsSuccess { get; }
        int StatusCode { get; }
        List<Error> Errors { get; set; }
        T Property { get; set; }
      //  Task<T> GetResponseData();
    }
}
