using System.Net.Http;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public class SetExpiryDateResponse : Result<SetExpiryDateResponse>
    {
        public SetExpiryDateResponse(HttpResponseMessage response) : base(response)
        {

        }
        public string ID { get; set; }
    }
}
