using Hopaut.Modules.Announcements.Domain;

namespace Hopaut.Modules.Announcements.Application;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Announcement>> GetByPostAsync(int postId, CancellationToken ct = default);
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
    Task DeleteAsync(Announcement announcement, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
