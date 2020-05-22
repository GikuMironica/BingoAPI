using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class UpdatePostResponse
    {
        public int Id { get; set; }

        public Int64 PostTime { get; set; }

        public Int64 EventTime { get; set; }
        public Location Location { get; set; }

        public string UserId { get; set; }

        public Event Event { get; set; }

        public IEnumerable<string>? Pictures { get; set; }
        #nullable enable
        public List<String>? Tags { get; set; }
    }
    public class UpdatedEvent
    {
        public int Id { get; set; }
    }
    public class UpdatedLocation
    {
        public int Id { get; set; }
#nullable enable
        public double? Logitude { get; set; }
        #nullable enable
        public double? Latitude { get; set; }
    }
}
