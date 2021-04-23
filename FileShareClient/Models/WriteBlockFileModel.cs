using System.Collections.Generic;

namespace FileShareClient.Models
{
    public class WriteBlockFileModel
    {
        public IEnumerable<string> BlockIds { get; set; }
    }
}