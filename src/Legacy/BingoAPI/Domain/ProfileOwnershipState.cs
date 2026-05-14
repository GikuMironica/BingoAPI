using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Domain
{
    public class ProfileOwnershipState
    {
        public bool Result { get; set; }
        public AppUser User { get; set; }
    }
}
