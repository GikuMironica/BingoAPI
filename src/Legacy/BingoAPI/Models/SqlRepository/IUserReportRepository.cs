using System.Collections.Generic;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IUserReportRepository : IRepository<UserReport>
    {
        public Task<bool> CanReport(string reporterId, string reportedId);
        public Task<List<UserReport>> GetAllAsync(string userId);
    }
}
