using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Queries;

/// <summary>Announcement reads served by no-tracking queries, filtered and paged in SQL (never loading aggregates).</summary>
public interface IAnnouncementReadModel
{
    Task<AnnouncementDto?> GetByIdAsync(AnnouncementId announcementId, CancellationToken ct);

    /// <summary>Every announcement of the post, newest first (the unpaged list, kept until plan Step 10.6).</summary>
    Task<IReadOnlyList<AnnouncementDto>> GetByPostAsync(PostId postId, CancellationToken ct);

    /// <summary>Up to <paramref name="count"/> announcements of the post, newest first, after the given position.</summary>
    Task<IReadOnlyList<AnnouncementDto>> GetPageAsync(PostId postId, AnnouncementKeyset? after, int count, CancellationToken ct);

    /// <summary>
    /// Up to <paramref name="count"/> of the given posts that have announcements, each with its newest announcement, the most
    /// recently announced first, after the given position.
    /// </summary>
    Task<IReadOnlyList<AnnouncementThread>> GetThreadsAsync(
        IReadOnlyCollection<PostId> postIds, AnnouncementThreadKeyset? after, int count, CancellationToken ct);
}
