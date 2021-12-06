using System.Collections.Generic;
using UKHO.FileShareAdminClient.Models.DTO;

namespace UKHO.FileShareAdminClient.Models
{
    public interface IBatchHandle
    {
        string BatchId { get; }

        bool IsSuccess { get; set; }

        List<Error> Errors { get; set; }
    }

    internal class BatchHandle : IBatchHandle
    {
        public string BatchId { get; }
        public bool IsSuccess { get; set; }
        public List<FileDetail> FileDetails { get; } = new List<FileDetail>();
        public List<Error> Errors { get; set; } = new List<Error>();

        public BatchHandle(string batchId)
        {
            BatchId = batchId;
        }

        internal void AddFile(string filename, string hash)
        {
            FileDetails.Add(new FileDetail {FileName = filename, Hash = hash});
        }
    }
}