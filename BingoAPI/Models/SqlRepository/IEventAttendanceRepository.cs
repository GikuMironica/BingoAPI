using BingoAPI.Domain;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IEventAttendanceRepository
    {
        public Task<AttendedEventResult> AttendEvent(AppUser user, int postId);
        public Task<bool> UnAttendEvent(AppUser user, int postId);
        public Task<List<Post>> GetActiveAttendedPostsByUserId(string userId);
        public Task<List<Post>> GetOldAttendedPostsByUserId(string userId);
        public Task<bool> IsUserAttendingEvent(string userId, int postId);
    }
}
