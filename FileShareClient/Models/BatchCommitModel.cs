using System.Collections.Generic;

namespace UKHO.FileShareClient.Models
{
    public class BatchCommitModel
    {
        public List<FileDetail> FileDetails { get; set; }
    }

    public class FileDetail
    {
        public string FileName { get; set; }
        public string Hash { get; set; }
    }
}