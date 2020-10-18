using Bingo.Contracts.Attributes;
using Bingo.Contracts.V1.Attributes;
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

        [MaxValue(15, ErrorMessage = "Maximum range is 15km")]
        [MinValue(1, ErrorMessage = "Minimum range is 1km")]
        [Required]
        public int RadiusRange { get; set; }

        
    }

}
