using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class PostResponse
    {
        public int Id { get; set; }

        public long PostTime { get; set; }

        public long EventTime { get; set; }
        public Location Location { get; set; }

        public string UserId { get; set; }

        public Event Event { get; set; }

        public IEnumerable<string>? Pictures { get; set; }

#nullable enable
        public List<String>? Tags { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int? Slots { get; set; }

        public double? EntrancePrice { get; set; }
#nullable enable
        public string? Requirements { get; set; }

        public int EventType { get; set; }
    }

    public class Location
    {
        public int Id { get; set; }
#nullable enable
        public double? Logitude { get; set; }

#nullable enable
        public double? Latitude { get; set; }

#nullable enable
        public string? Address { get; set; }

#nullable enable
        public string? City { get; set; }

#nullable enable
        public string? Region { get; set; }

#nullable enable
        public string? Country { get; set; }
    }
}
