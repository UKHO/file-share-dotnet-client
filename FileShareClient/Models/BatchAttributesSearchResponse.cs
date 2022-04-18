using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace UKHO.FileShareClient.Models
{
    [DataContract]
    public class BatchAttributesSearchResponse : IEquatable<BatchAttributesSearchResponse>
    {
        public BatchAttributesSearchResponse(int searchBatchCount = default, List<Attributes> batchAttributes = default)
        {
            SearchBatchCount = searchBatchCount;
            BatchAttributes = batchAttributes;
        }

        [DataMember(Name = "searchBatchCount", EmitDefaultValue = false)]
        public int? SearchBatchCount { get; set; }

        [DataMember(Name = "batchAttributes", EmitDefaultValue = false)]
        public List<Attributes> BatchAttributes { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class BatchAttributesSearchResponse {\n");
            sb.Append("  SearchBatchCount: ").Append(SearchBatchCount).Append("\n");
            sb.Append("  BatchAttributes: ").Append(BatchAttributes).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        public override bool Equals(object input)
        {
            return Equals(input as BatchAttributesSearchResponse);
        }

        public bool Equals(BatchAttributesSearchResponse input)
        {
            if (input == null)
                return false;

            return
                (
                    SearchBatchCount == input.SearchBatchCount ||
                    SearchBatchCount != null &&
                    SearchBatchCount.Equals(input.SearchBatchCount)
                ) &&
                (
                    BatchAttributes == input.BatchAttributes ||
                    BatchAttributes != null &&
                    input.BatchAttributes != null);
                
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                if (SearchBatchCount != null)
                    hashCode = hashCode * 59 + SearchBatchCount.GetHashCode();
                if (BatchAttributes != null)
                    hashCode = hashCode * 59 + BatchAttributes.GetHashCode();
                return hashCode;
            }
        }

    }
}
