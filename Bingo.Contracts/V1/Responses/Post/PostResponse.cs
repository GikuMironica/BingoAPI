using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Responses.Post
{
    public class PostResponse
    {
        public int Id { get; set; }

        public Int64 PostTime { get; set; }

        public Int64 EventTime { get; set; }

        public Int64? EndTime { get; set; }

        public int ActiveFlag { get; set; }

        public Location Location { get; set; }

        public string UserId { get; set; }

        public double HostRating { get; set; }

        public bool IsAttending { get; set; }

        public int AvailableSlots { get; set; }

        public Event Event { get; set; }
        public int RepeatablePropertyDataId { get; set; }
        public int VoucherDataId { get; set; }
        public int AnnouncementsDataId { get; set; }
        public int AttendanceDataId { get; set; }

        public IEnumerable<string>? Pictures { get; set; }

#nullable enable
        public List<String>? Tags { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int? Slots { get; set; }

        public string? Title { get; set; }

        public double? EntrancePrice { get; set; }
#nullable enable
        public string? Requirements { get; set; }

        public int EventType { get; set; }
    }

    public class Location
    {
        public int Id { get; set; }
#nullable enable
        public double? Longitude { get; set; }
#nullable enable
        public double? Latitude { get; set; }

        public string? EntityName { get; set; }
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
