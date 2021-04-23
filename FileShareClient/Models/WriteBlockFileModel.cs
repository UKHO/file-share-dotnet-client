using System.Collections.Generic;

namespace UKHO.FileShareClient.Models
{
    public class WriteBlockFileModel
    {
        public IEnumerable<string> BlockIds { get; set; }
    }
}