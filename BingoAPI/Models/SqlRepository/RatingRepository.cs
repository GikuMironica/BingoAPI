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
        private readonly DataContext _context;

        public RatingRepository(DataContext context)
        {
            this._context = context;
        }

        public async Task<bool> AddAsync(Rating entity)
        {
            await _context.Rating.AddAsync(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rating = await _context.Rating.SingleOrDefaultAsync(r => r.Id == id);
            if (rating == null)
                return false;

            _context.Rating.Remove(rating);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Rating>> GetAllAsync(string userId)
        {
            return await _context.Rating
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        
        public async Task<Rating> GetByIdAsync(int id)
        {
            return await _context.Rating
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasAlreadyRatedAsync(string requesterId, int postId)
        {
            var result = await _context.Rating
                .Where(r => r.RaterId == requesterId && r.PostId == postId)
                .CountAsync();

            return result > 0;
        }

        public async Task<bool> IsRatingOwnerAsync(string requesterId, int ratingId)
        {
            var result = await _context.Rating
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
            var ratings = await _context.Rating
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
