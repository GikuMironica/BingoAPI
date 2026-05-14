using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IRatingRepository : IRepository<Rating>
    {
        public Task<bool> HasAlreadyRatedAsync(string requesterId, int postId);
        public Task<bool> IsRatingOwnerAsync(string requesterId, int ratingId);
        public Task<List<Rating>> GetAllAsync(string userId);
        public Task<double> GetUserRating(string userId);
    }
}
