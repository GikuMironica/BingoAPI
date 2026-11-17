using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Announcements.Application;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

public sealed class AnnouncementRepository(AnnouncementsModuleDbContext db) : IAnnouncementRepository
{
    public async Task<AnnouncementId> NextIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(AnnouncementsModuleDbContext.SchemaName, AnnouncementsModuleDbContext.AnnouncementIdSequence, ct));

    public Task<Announcement?> GetByIdAsync(AnnouncementId id, CancellationToken ct = default)
        => db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Announcement>> GetByPostAsync(PostId postId, CancellationToken ct = default)
        => db.Announcements.Where(a => a.PostId == postId).OrderByDescending(a => a.CreatedAt).ToListAsync(ct);

    public void Add(Announcement announcement) => db.Announcements.Add(announcement);

    public void Remove(Announcement announcement) => db.Announcements.Remove(announcement);
}
