using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IPostsRepository : IRepository<Post>
    {
        public Task<bool> IsPostOwnerOrAdminAsync(int postId, string userId);

        public Task<Post> GetPostByIdAsync(int postId);
    }
}
