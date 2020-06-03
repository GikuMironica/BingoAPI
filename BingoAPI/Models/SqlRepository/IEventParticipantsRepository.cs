using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IEventParticipantsRepository
    {
        public Task<bool> AcceptAttendee(string userId, int postId);
        public Task<bool> RejectAttendee(string userId, int postId);
        public Task<List<AppUser>> DisplayAll(int postId, PaginationFilter paginationFilter = null);
        public Task<List<AppUser>> DisplayAllAccepted(int postId);
        public Task<List<AppUser>> DisplayAllPending(int postId);
        public Task<bool> IsPostOwnerAsync(int postId, string userId);
    }
}
