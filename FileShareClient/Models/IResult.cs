using System.Collections.Generic;

namespace UKHO.FileShareClient.Models
{
    public interface IResult<T>
    {
        bool IsSuccess { get; }
        int StatusCode { get; }
        List<Error> Errors { get; set; }
        T Data { get; set; }
    }
}
