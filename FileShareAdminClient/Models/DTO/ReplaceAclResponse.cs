using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models.DTO
{
    public class ReplaceAclResponse : IBatchHandle
    {
        public string BatchId { get; set; }

        public bool IsSuccess { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public ReplaceAclResponse(string batchId)
        {
            BatchId = batchId;
        }
    }
}
