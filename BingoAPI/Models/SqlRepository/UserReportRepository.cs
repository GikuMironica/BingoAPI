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
        private readonly DataContext _context;
        private const long OneWeekSeconds = 604800;

        public UserReportRepository(DataContext context)
        {
            this._context = context;
        }

        public async Task<bool> AddAsync(UserReport entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
            await _context.UserReports.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }

       
        public async Task<bool> DeleteAsync(int id)
        {
            var report = await GetByIdAsync(id);
            if (report == null)
                return false;
            _context.UserReports.Remove(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<IEnumerable<UserReport>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<UserReport> GetByIdAsync(int id)
        {
            return await _context.UserReports
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CanReport(string reporterId, string reportedId)
        {

            // TODO - to document feature specs
            var time = await _context.UserReports
                .Where(ur => ur.ReporterId == reporterId && ur.ReportedUserId == reportedId)
                .OrderByDescending(ur => ur.Timestamp)
                .AsNoTracking()
                .Select(ur => ur.Timestamp)
                .FirstOrDefaultAsync();                
                

            var elapsedTime = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds() - time;
            return elapsedTime >= OneWeekSeconds;
        }

        public Task<bool> UpdateAsync(UserReport entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<UserReport>> GetAllAsync(string userId)
        {
            return await _context.UserReports
                .Where(ur => ur.ReportedUserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
