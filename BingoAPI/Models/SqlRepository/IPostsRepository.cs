using Bingo.Contracts.V1.Requests.Post;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IPostsRepository : IRepository<Post>
    {
        public Task<bool> IsPostOwnerOrAdminAsync(int postId, string userId);
        public Task<IEnumerable<Post>> GetAllAsync(Point location, int radius);
        public Task<Post> GetByIdAsync(int postId);
        public Task<Post> GetPlainPostAsync(int postId);
        public Task<List<string>> GetParticipantsIdAsync(int postId);
        public Task<bool> IsHostIdPostOwner(string hostId, int postId);

    }
}
