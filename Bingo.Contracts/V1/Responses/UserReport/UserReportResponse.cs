using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.UserReport
{
    public class UserReportResponse
    {
        public int Id { get; set; }

        public Int64 Timestamp { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }

        public string ReporterId { get; set; }

        public string ReportedUserId { get; set; }
    }
}
