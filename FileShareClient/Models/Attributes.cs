using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace UKHO.FileShareClient.Models
{
    public class Attributes : IEquatable<Attributes> 
    {
        public Attributes(string key = default, string[] values = default)
        {
            Key = key;
            Values = values;
        }

        [DataMember(Name = "key", EmitDefaultValue = false)]
        public string Key { get; set; }

        [DataMember(Name = "values", EmitDefaultValue = false)]
        public string[] Values { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class BatchAttributesSearchResponse {\n");
            sb.Append("  Key: ").Append(Key).Append("\n");
            sb.Append("  Values: ").Append(Values).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public override bool Equals(object input)
        {
            return Equals(input as Attributes);
        }

        public bool Equals(Attributes input)
        {
            if (input == null)
                return false;

            return
                (
                    Key == input.Key ||
                    Key != null &&
                    Key.Equals(input.Key)
                ) &&
                (
                    Values == input.Values ||
                    Values != null &&
                    input.Values != null);

        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                if (Key != null)
                    hashCode = hashCode * 59 + Key.GetHashCode();
                if (Values != null)
                    hashCode = hashCode * 59 + Values.GetHashCode();
                return hashCode;
            }
        }
    }
}
