using System.Net.Http;

namespace UKHO.FileShareAdminClient.Models.Response
{
    public class SetExpiryDateResponse : Result<SetExpiryDateResponse>
    {
        public string ID { get; set; }
    }
}
