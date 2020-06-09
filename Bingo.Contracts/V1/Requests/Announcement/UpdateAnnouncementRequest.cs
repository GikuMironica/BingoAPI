using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Announcement
{
    public class UpdateAnnouncementRequest
    {
        public String? Message { get; set; }

        public Int64? Timestamp { get; set; }
    }
}
