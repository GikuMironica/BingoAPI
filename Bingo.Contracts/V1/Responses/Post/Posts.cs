using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class Posts
    {
        public int PostId { get; set; }
        public int PostType { get; set; }
        public string Thumbnail { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
        public double HostRating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RepteatableEnabled { get; set; }
        public int Frequency { get; set; }
        public int VouchersEnabled { get; set; }
        public Int64 PostTime { get; set; }
        public Int64 StartTime { get; set; }
        public Int64 EndTime { get; set; }
        public double EntracePrice { get; set; }
        public int Slots { get; set; }

    }
}
