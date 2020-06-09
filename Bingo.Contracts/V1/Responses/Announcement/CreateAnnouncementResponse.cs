using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Announcement
{
    public class CreateAnnouncementResponse
    {
        public int Id { get; set; }

        public int PostId { get; set; }

        public String Message { get; set; }

        public Int64 Timestamp { get; set; }
    }
}
