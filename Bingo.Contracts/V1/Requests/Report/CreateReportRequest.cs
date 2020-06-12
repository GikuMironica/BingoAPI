using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Report
{
    public class CreateReportRequest
    {
        public string Reason { get; set; }

        public string Message { get; set; }

        public int PostId { get; set; }
    }
}
