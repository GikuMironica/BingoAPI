using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly DataContext context;

        public AnnouncementRepository(DataContext context)
        {
            this.context = context;
        }


        public async Task<bool> AddAsync(Announcement entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await context.Announcements.AddAsync(entity);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var announcement = await context.Announcements.SingleOrDefaultAsync(x => x.Id == Id);
            context.Announcements.Remove(announcement);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<List<Announcement>> GetAllByPostIdAsync(int id)
        {
            return await context.Announcements
                .Where(a => a.PostId == id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Announcement> GetByIdAsync(int id)
        {
            return await context.Announcements
                   .Where(a => a.Id == id)
                   .SingleOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(Announcement entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            context.Announcements.Update(entity);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<Announcement> GetByIdNoTrackAsync(int id)
        {
            return await context.Announcements
                .Where(a => a.Id == id)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
               
        public Task<IEnumerable<Announcement>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
