using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models
{
    public class FileModel
    {
        public IEnumerable<KeyValuePair<string, string>> Attributes { get; set; }
    }
}