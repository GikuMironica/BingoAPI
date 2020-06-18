using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class UpdatePostRequest
    {
        #nullable enable
        public Int64? EventTime { get; set; }

        public long? EndTime { get; set; }

        #nullable enable
        public UpdatedCompleteLocation? UserLocation { get; set; }

        public IFormFile? Picture1 { get; set; }

        public IFormFile? Picture2 { get; set; }

        public IFormFile? Picture3 { get; set; }

        public UpdatedEvent? Event { get; set; }

        public List<String>? RemainingImagesGuids { get; set; }
        #nullable enable
        [MaxLength(20)]
        public List<String>? TagNames { get; set; }
    }

    public class UpdatedCompleteLocation
    {
        [Range(-180.000000000000000000000000,180.000000000000000000000000)]
        public double? Longitude { get; set; }
        [Range(-90.0000000000000000000000000,90.0000000000000000000000000)]
        public double? Latitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? EntityName { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
    }
    public class UpdatedEvent
    {
        [MinLength(10)]
        [MaxLength(5000)]
        public string? Description { get; set; }
        #nullable enable
        public string? Requirements { get; set; }
        public int? Slots { get; set; }
        public double? EntrancePrice { get; set; }
        public string? Title { get; set; }
    }

}
