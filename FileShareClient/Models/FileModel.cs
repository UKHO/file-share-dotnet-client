using System.Collections.Generic;

namespace UKHO.FileShareClient.Models
{
    public class FileModel
    {
        public IEnumerable<KeyValuePair<string, string>> Attributes { get; set; }
    }
}