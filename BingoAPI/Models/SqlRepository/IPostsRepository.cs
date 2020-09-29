using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Domain;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IPostsRepository : IRepository<Post>
    {
        public Task<int> GetAvailableSlotsAsync(int postId);
        public Task<bool> IsPostOwnerOrAdminAsync(int postId, string userId);
        public Task<IEnumerable<Post>> GetAllAsync(Point location, int radius, GetPostsFilter postsFilter, Int64 today, string Tag = "%");
        public Task<Post> GetByIdAsync(int postId);
        public Task<bool> DisablePost(Post postId);
        public Task<Post> GetPlainPostAsync(int postId);
        public Task<List<string>> GetParticipantsIdAsync(int postId);
        public Task<bool> IsHostIdPostOwner(string hostId, int postId);
        public Task<string> GetHostId(int postId);
        public Task<int> GetEventType(int postId);
        public Task<int> GetActiveEventsNumbers(string userId);
        public Task<IEnumerable<Post>> GetMyActive(string userId, PaginationFilter paginationFilter);
        public Task<IEnumerable<Post>> GetMyInactive(string userId, PaginationFilter paginationFilter);

    }
}
