using System;
using System.Collections.Generic;
using System.Text;

namespace Bingo.Contracts.V1.Requests.Post
{
    public class GetAllRequest
    {
        public int RadiusRange { get; set; }

        public UserLocation UserLocation { get; set; }
    }

    public class UserLocation
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
