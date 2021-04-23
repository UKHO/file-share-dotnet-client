using System.Collections.Generic;

namespace FileShareClient.Models
{
    public class FileModel
    {
        public IEnumerable<KeyValuePair<string, string>> Attributes { get; set; }
    }
}