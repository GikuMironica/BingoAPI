using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        public Task<Announcement> GetByIdNoTrackAsync(int id);
        public Task<List<Announcement>> GetAllByPostIdAsync(int id);
    }
}
