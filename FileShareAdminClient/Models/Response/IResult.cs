using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public interface IResult<T>
    {
        bool IsSuccess { get; }
        int StatusCode { get; }
        List<Error> Errors { get; set; }
    }
}
