using BingoAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class BugReportRepository : IBugReportRepository
    {
        private readonly DataContext _context;

        public BugReportRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Bug entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
            await _context.Bugs.AddAsync(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<bool> DeleteAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Bug>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Bug> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(Bug entity)
        {
            throw new NotImplementedException();
        }
    }
}
