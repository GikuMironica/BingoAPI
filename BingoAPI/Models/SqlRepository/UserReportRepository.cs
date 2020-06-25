using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly DataContext context;
        private readonly long _oneWeekSeconds = 604800;

        public UserReportRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddAsync(UserReport entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await context.UserReports.AddAsync(entity);
            return await context.SaveChangesAsync() > 0;
        }

       
        public async Task<bool> DeleteAsync(int Id)
        {
            var report = await GetByIdAsync(Id);
            if (report == null)
                return false;
            context.UserReports.Remove(report);
            return await context.SaveChangesAsync() > 0;
        }

        public Task<IEnumerable<UserReport>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UserReport> GetByIdAsync(int id)
        {
            return await context.UserReports
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CanReport(string reporterId, string reportedId)
        {
            var time = await context.UserReports
                .Where(ur => ur.ReporterId == reporterId && ur.ReportedUserId == reportedId)
                .OrderByDescending(ur => ur.Timestamp)
                .AsNoTracking()
                .Select(ur => ur.Timestamp)
                .FirstOrDefaultAsync();                
                

            var elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - time;
            return elapsedTime >= _oneWeekSeconds;
        }

        public Task<bool> UpdateAsync(UserReport entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserReport>> GetAllAsync(string userId)
        {
            return await context.UserReports
                .Where(ur => ur.ReportedUserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
