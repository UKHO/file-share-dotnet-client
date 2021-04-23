using System;

namespace FileShareClient.Models
{
    public class BatchStatusResponse
    {
        public Guid BatchId { get; set; }
        public string Status { get; set; }
    }
}