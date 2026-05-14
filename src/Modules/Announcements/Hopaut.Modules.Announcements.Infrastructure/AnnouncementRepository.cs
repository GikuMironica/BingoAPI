using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AnnouncementsModuleDbContext _db;

    public AnnouncementRepository(AnnouncementsModuleDbContext db) => _db = db;

    public Task<Announcement?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Announcement>> GetByPostAsync(int postId, CancellationToken ct = default)
        => _db.Announcements.Where(a => a.PostId == postId).OrderByDescending(a => a.Timestamp).ToListAsync(ct);

    public async Task AddAsync(Announcement announcement, CancellationToken ct = default)
        => await _db.Announcements.AddAsync(announcement, ct);

    public Task DeleteAsync(Announcement announcement, CancellationToken ct = default)
    {
        _db.Announcements.Remove(announcement);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
