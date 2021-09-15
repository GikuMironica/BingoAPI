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
        private readonly DataContext _context;

        public AnnouncementRepository(DataContext context)
        {
            this._context = context;
        }


        public async Task<bool> AddAsync(Announcement entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _context.Announcements.AddAsync(entity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var announcement = await _context.Announcements.SingleOrDefaultAsync(x => x.Id == id);
            _context.Announcements.Remove(announcement);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Announcement>> GetAllByPostIdAsync(int id)
        {
            return await _context.Announcements
                .Where(a => a.PostId == id)
                .OrderBy(a => a.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Announcement> GetByIdAsync(int id)
        {
            return await _context.Announcements
                   .Where(a => a.Id == id)
                   .SingleOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(Announcement entity)
        {
            entity.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _context.Announcements.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Announcement> GetByIdNoTrackAsync(int id)
        {
            return await _context.Announcements
                .Where(a => a.Id == id)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<List<Post>> GetEventsWithOutbox(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Announcements)
                .Where(p => p.Announcements.Count > 0)
                .OrderByDescending(p => p.Announcements
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefault())
                .Take(30)
                .ToListAsync();
        }

        public async Task<List<Post>> GetAttendedEventsWithAnnouncements(string userId)
        {
            return await _context.Participations
                .Where(p => p.UserId == userId && p.Accepted == 1)
                .Include(p => p.Post)
                .Include(p => p.Post.Location)
                .Include(p => p.Post.Event)
                .Include(p => p.Post.Announcements)
                .Select(p => p.Post)
                .Where(p => p.Announcements.Count > 0)
                .OrderByDescending(p => p.Announcements
                    .OrderByDescending(a => a.Timestamp)
                    .FirstOrDefault())
                .Take(30)
                .ToListAsync();
        }

        public Task<IEnumerable<Announcement>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
