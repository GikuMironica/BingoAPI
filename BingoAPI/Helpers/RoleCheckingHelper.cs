using BingoAPI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Helpers
{
    public static class RoleCheckingHelper
    {
        public static async Task<bool> CheckIfAdmin(UserManager<AppUser> userManager, AppUser user)
        {
            var requesterRoles = await userManager.GetRolesAsync(user);
            foreach (var role in requesterRoles)
            {
                if (role == "Admin" || role == "SuperAdmin")
                    return true;
            }
            return false;
        } 
    }
}
