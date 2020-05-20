using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class UpdatePostRequest
    {
        #nullable enable
        public long? EventTime { get; set; }

        #nullable enable
        public long? PostTime { get; set; }

        #nullable enable
        public UpdatedCompleteLocation? UserLocation { get; set; }

        public IFormFile? Picture1 { get; set; }

        public IFormFile? Picture2 { get; set; }

        public IFormFile? Picture3 { get; set; }

        public IFormFile? Picture4 { get; set; }

        public IFormFile? Picture5 { get; set; }

        public UpdatedEvent? Event { get; set; }

        public List<string>? RemainingImagesGuids { get; set; }
#nullable enable
        public List<String>? TagNames { get; set; }
    }

    public class UpdatedCompleteLocation
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Country { get; set; }
    }
    public class UpdatedEvent
    {
        public string? Description { get; set; }
        #nullable enable
        public string? Requirements { get; set; }
        public int? Slots { get; set; }
        public double? EntrancePrice { get; set; }
    }

}
