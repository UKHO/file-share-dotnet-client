using System.Net.Http;

namespace UKHO.FileShareAdminClient.Models.DTO
{
    public class SetExpiryDateResponse : ErrorDescriptionModel
    {
        public SetExpiryDateResponse(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
        public bool IsSuccess { get; set; } 
    }
}
