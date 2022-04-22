using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace UKHO.FileShareClient.Models
{
    public class SearchBatchAttribute 
    {
        public SearchBatchAttribute(string key = default, List<string> values = default)
        {
            Key = key;
            Values = values;
        }

        [DataMember(Name = "key", EmitDefaultValue = false)]
        public string Key { get; set; }

        [DataMember(Name = "values", EmitDefaultValue = false)]
        public List<string> Values { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"class {nameof(SearchBatchAttribute)} {"{"}\n");
            sb.Append($"{nameof(Key)}: {(Key)}\n");
            sb.Append($"{nameof(Values)}: {string.Join(", ", Values)}\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
