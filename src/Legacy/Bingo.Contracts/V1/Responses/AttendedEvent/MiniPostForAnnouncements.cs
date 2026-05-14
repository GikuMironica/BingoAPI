using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.AttendedEvent
{
    public class MiniPostForAnnouncements
    {        
            public int PostId { get; set; }
            public int PostType { get; set; }
            public string Thumbnail { get; set; }
            public string LastMessage { get; set; }
            public Int64? LastMessageTime { get; set; }
            public string Title { get; set; }       
    }
}
