using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models
{
    public interface IBatchHandle
    {
        string BatchId { get; }
        List<FileDetail> FileDetails { get; }
        void AddFile(string filename, string hash);
    }

    internal class BatchHandle : IBatchHandle
    {
        public string BatchId { get; }

        public List<FileDetail> FileDetails { get; } = new List<FileDetail>();

        public BatchHandle(string batchId)
        {
            BatchId = batchId;
        }

        public void AddFile(string filename, string hash)
        {
            FileDetails.Add(new FileDetail { FileName = filename, Hash = hash });
        }
    }
}