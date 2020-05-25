using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class GetAllRequest
    {
        
        public UserLocation UserLocation { get; set; }
    }

    public class UserLocation
    {
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
        public int? RadiusRange { get; set; }
    }
}
