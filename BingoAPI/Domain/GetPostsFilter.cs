using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class GetPostsFilter
    {
        public bool? HouseParty { get; set; } = false;
        public bool? Club { get; set; } = false;
        public bool? Bar { get; set; } = false;
        public bool? BikerMeet { get; set; } = false;
        public bool? BicycleMeet { get; set; } = false;
        public bool? CarMeet { get; set; } = false;
        public bool? StreetParty { get; set; } = false;
        public bool? Marathon { get; set; } = false;
        public bool? Other { get; set; } = false;
    }
}
