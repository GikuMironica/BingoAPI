using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IReportsRepository : IRepository<Report>
    {
        public Task<bool> HasAlreadyReported(string reporterId, int postId);
        public Task<List<Report>> GetAllAsync(string userId);
        public Task<bool> DeleteAllForUserAsync(string userId);
    }
}
