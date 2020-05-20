using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class CreatePostRequest
    {

        public long EventTime { get; set; }

        public long PostTime { get; set; }

        public UserCompleteLocation UserLocation { get; set; }

        public IFormFile? Picture1 { get; set; }

        public IFormFile? Picture2 { get; set; }

        public IFormFile? Picture3 { get; set; }

        public IFormFile? Picture4 { get; set; }

        public IFormFile? Picture5 { get; set; }

        //public List<IFormFile>? Pictures { get; set; }

        public ContainedEvent Event { get; set; }

        #nullable enable
        public List<String>? Tags { get; set; }
    }

    public class UserCompleteLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? Country { get; set; }
    }

    public class ContainedEvent
    {
        public string Description { get; set; }
        #nullable enable
        public string? Requirements { get; set; }
        public int? Slots { get; set; }
        public double? EntrancePrice { get; set; }
        public int EventType { get; set; }
    }


}
