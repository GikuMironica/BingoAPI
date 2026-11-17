using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application;

/// <summary>Announcement aggregate store. Changes are saved by the Announcements unit of work, never by the repository.</summary>
public interface IAnnouncementRepository
{
    /// <summary>Next ID from the announcements sequence, assigned before the announcement is created.</summary>
    Task<AnnouncementId> NextIdAsync(CancellationToken ct = default);
    Task<Announcement?> GetByIdAsync(AnnouncementId id, CancellationToken ct = default);
    Task<List<Announcement>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    void Add(Announcement announcement);
    void Remove(Announcement announcement);
}
