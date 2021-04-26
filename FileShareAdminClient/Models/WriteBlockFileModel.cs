using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models
{
    public class WriteBlockFileModel
    {
        public IEnumerable<string> BlockIds { get; set; }
    }
}