using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class CreatePostRequest
    {
        [Required]
        public long EventTime { get; set; }

        [Required]
        public long? EndTime { get; set; }

        public UserCompleteLocation UserLocation { get; set; }

        public IFormFile? Picture1 { get; set; }

        public IFormFile? Picture2 { get; set; }

        public IFormFile? Picture3 { get; set; }

        //public List<IFormFile>? Pictures { get; set; }

        public ContainedEvent Event { get; set; }

        [MaxLength(20)]
        #nullable enable
        public List<string>? Tags { get; set; }
    }

    public class UserCompleteLocation
    {
        [Required]
        public double Longitude { get; set; }

        [Required]
        public double Latitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? EntityName { get; set; }
        public string? Country { get; set; }
    }

    public class ContainedEvent
    {
        [Required]
        [MaxLength(3000)]
        [MinLength(10)]
        public string Description { get; set; }
#nullable enable
        [MaxLength(500)]
        public string? Requirements { get; set; }
        public int? Slots { get; set; }
        [Required]
        public string Title { get; set; }
        public double? EntrancePrice { get; set; }
        [Required]
        public int EventType { get; set; }
    }


}
