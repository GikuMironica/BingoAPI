using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfilePicture { get; set; }

        public string Description { get; set; }

        public List<Post>? Posts { get; set; }
        //todo
        public List<Participation> AttendedEvents { get; set; }
        //todo
        public List<Rating> Ratings { get; set; }
        //todo
        public List<UserVoucher> Vouchers { get; set; }
    }
}
