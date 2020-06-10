using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class RepostsRepository : IReportsRepository
    {
        private readonly DataContext context;

        public RepostsRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddAsync(Report entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await context.Reports.AddAsync(entity);
            return await context.SaveChangesAsync() > 0;
        }

        public Task<bool> DeleteAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Report>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Report>> GetAllAsync(string userId)
        {
            return await context.Reports
                .Where(r => r.UserId == userId);
        }

        public async Task<Report> GetByIdAsync(int id)
        {
            return await context.Reports
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasAlreadyReported(string reporterId, int postId)
        {
            int result = await context.Reports
                .Where(r => r.ReporterId == reporterId && r.PostId == postId)
                .CountAsync();

            return result > 0;
        }


        public Task<bool> UpdateAsync(Report entity)
        {
            throw new NotImplementedException();
        }
    }
}
