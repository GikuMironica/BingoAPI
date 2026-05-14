using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.AttendedEvent
{
    public class ActiveAttendedEvent
    {
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public Int64 EventTime { get; set; }
        public double Price { get; set; }
        public string Address { get; set; }

    }
}
