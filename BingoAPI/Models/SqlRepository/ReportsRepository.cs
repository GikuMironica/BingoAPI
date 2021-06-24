using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly DataContext _context;

        public ReportsRepository(DataContext context)
        {
            this._context = context;
        }

        public async Task<bool> AddAsync(Report entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _context.Reports.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAllForUserAsync(string userId)
        {
            var report = await _context.Reports
                .Where(r => r.ReportedHostId == userId)
                .ToListAsync();

            if (report.Count == 0)
                return false;

            _context.Reports.RemoveRange(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var report = await _context.Reports.SingleOrDefaultAsync(r => r.Id == id);
            if (report == null)
                return false;

            _context.Reports.Remove(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<IEnumerable<Report>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<List<Report>> GetAllAsync(string userId)
        {
            return await _context.Reports
                .Where(r => r.ReportedHostId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Report> GetByIdAsync(int id)
        {
            return await _context.Reports
                .Where(r => r.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> HasAlreadyReported(string reporterId, int postId)
        {
            var result = await _context.Reports
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
