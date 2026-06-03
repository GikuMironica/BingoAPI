using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(AnnouncementId id, CancellationToken ct = default);
    Task<List<Announcement>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
    Task DeleteAsync(Announcement announcement, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
