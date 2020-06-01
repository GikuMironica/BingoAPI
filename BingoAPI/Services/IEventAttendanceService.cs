using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IEventAttendanceService
    {
        public Task<bool> AttendEvent(AppUser user, int postId);
        public Task<bool> UnAttendEvent(AppUser user, int postId);
        public Task<List<Post>> GetActiveAttendedPostsByUserId(string userId);
        public Task<List<Post>> GetOldAttendedPostsByUserId(string userId);
    }
}
