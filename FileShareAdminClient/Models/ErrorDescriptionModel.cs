using System;
using System.Collections.Generic;
using System.Text;

namespace UKHO.FileShareAdminClient.Models
{
    public class ErrorDescriptionModel
    {
        public IEnumerable<Error> Errors { get; set; }
    }

    public class Error
    {
        public string Source { get; set; }
        public string Description { get; set; }
    }

    public class ErrorDescription
    {
        public string CorrelationId { get; set; }
        public List<Error> Errors { get; set; }

    }
}
