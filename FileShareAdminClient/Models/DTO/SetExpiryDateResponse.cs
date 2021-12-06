﻿using System.Collections.Generic;

namespace UKHO.FileShareAdminClient.Models.DTO
{
    public class SetExpiryDateResponse : IBatchHandle
    {
        public string BatchId { get; set; }

        public bool IsSuccess { get; set; }

        public List<Error> Errors { get; set; } = new List<Error>();

        public SetExpiryDateResponse(string batchId)
        {
            BatchId = batchId;
        }
    }
}
