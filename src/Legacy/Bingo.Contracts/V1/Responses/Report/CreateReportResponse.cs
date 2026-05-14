using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Report
{
    public class CreateReportResponse
    {
        public int Id { get; set; }

        public Int64 Timestamp { get; set; }

        public int Reason { get; set; }

        public string Message { get; set; }
    }
}
