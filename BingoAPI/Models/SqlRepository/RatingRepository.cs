using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class RatingRepository : IRatingRepository
    {
        private readonly DataContext context;

        public RatingRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddAsync(Rating entity)
        {
            await context.Rating.AddAsync(entity);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var rating = await context.Rating.SingleOrDefaultAsync(r => r.Id == Id);
            if (rating == null)
                return false;

            context.Rating.Remove(rating);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<Rating>> GetAllAsync(string userId)
        {
            return await context.Rating
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        
        public async Task<Rating> GetByIdAsync(int id)
        {
            return await context.Rating
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasAlreadyRatedAsync(string requesterId, string hostId, int postId)
        {
            var result = await context.Rating
                .Where(r => r.UserId == hostId && r.RaterId == requesterId && r.PostId == postId)
                .CountAsync();

            return result > 0;
        }

        public async Task<bool> IsRatingOwnerAsync(string requesterId, int ratingId)
        {
            var result = await context.Rating
                .Where(r => r.Id == ratingId && r.UserId == requesterId)
                .CountAsync();

            return result > 0;
        }

        public Task<bool> UpdateAsync(Rating entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Rating>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<double> GetUserRating(string userId)
        {
            var ratings = await context.Rating
                .Where(r => r.UserId == userId)
                .Select(r => r.Rate)
                .ToListAsync();

            if (ratings.Any())
            {
                return ratings.Average();
            }

            return 0;
        }
    }
}
