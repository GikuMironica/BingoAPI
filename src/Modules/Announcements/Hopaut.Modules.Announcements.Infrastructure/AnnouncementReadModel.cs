using Hopaut.Modules.Announcements.Application.Queries;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Announcements.Infrastructure;

/// <summary>
/// Serves <see cref="IAnnouncementReadModel"/> with no-tracking queries. Pages are cut in SQL with keyset row comparisons,
/// which LINQ cannot express on value-converted IDs (decision D-029).
/// </summary>
public sealed class AnnouncementReadModel(AnnouncementsModuleDbContext db) : IAnnouncementReadModel
{
    public Task<AnnouncementDto?> GetByIdAsync(AnnouncementId announcementId, CancellationToken ct) =>
        db.Announcements
            .AsNoTracking()
            .Where(a => a.Id == announcementId)
            .Select(a => new AnnouncementDto(a.Id, a.PostId, a.Message.Value, a.CreatedAt, a.EditedAt))
            .SingleOrDefaultAsync(ct);

    // The PostId index serves the filter.
    public async Task<IReadOnlyList<AnnouncementDto>> GetByPostAsync(PostId postId, CancellationToken ct) =>
        await db.Announcements
            .AsNoTracking()
            .Where(a => a.PostId == postId)
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Select(a => new AnnouncementDto(a.Id, a.PostId, a.Message.Value, a.CreatedAt, a.EditedAt))
            .ToListAsync(ct);

    // TODO: add index — CREATE INDEX IX_announcements_PostId_CreatedAt_Id ON announcements.announcements ("PostId", "CreatedAt" DESC, "Id" DESC);
    public async Task<IReadOnlyList<AnnouncementDto>> GetPageAsync(
        PostId postId, AnnouncementKeyset? after, int count, CancellationToken ct)
    {
        var rows = await db.Database.SqlQuery<AnnouncementRow>($"""
            SELECT a."Id", a."PostId", a."Message", a."CreatedAt", a."EditedAt"
            FROM announcements.announcements AS a
            WHERE a."PostId" = {postId.Value}
              AND ({after is null} OR (a."CreatedAt", a."Id") < ({after?.CreatedAt ?? DateTimeOffset.UnixEpoch}, {after?.AnnouncementId.Value ?? 0}))
            ORDER BY a."CreatedAt" DESC, a."Id" DESC
            LIMIT {count}
            """).ToListAsync(ct);

        return rows.ConvertAll(ToDto);
    }

    // The index proposed above serves DISTINCT ON; the posts are bounded by the caller's attended or hosted posts.
    public async Task<IReadOnlyList<AnnouncementThread>> GetThreadsAsync(
        IReadOnlyCollection<PostId> postIds, AnnouncementThreadKeyset? after, int count, CancellationToken ct)
    {
        if (postIds.Count == 0)
            return [];

        var ids = postIds.Select(id => id.Value).ToArray();
        var rows = await db.Database.SqlQuery<AnnouncementRow>($"""
            SELECT latest."Id", latest."PostId", latest."Message", latest."CreatedAt", latest."EditedAt"
            FROM (
                SELECT DISTINCT ON (a."PostId") a."Id", a."PostId", a."Message", a."CreatedAt", a."EditedAt"
                FROM announcements.announcements AS a
                WHERE a."PostId" = ANY({ids})
                ORDER BY a."PostId", a."CreatedAt" DESC, a."Id" DESC
            ) AS latest
            WHERE {after is null} OR (latest."CreatedAt", latest."PostId") < ({after?.LastAnnouncedAt ?? DateTimeOffset.UnixEpoch}, {after?.PostId.Value ?? 0})
            ORDER BY latest."CreatedAt" DESC, latest."PostId" DESC
            LIMIT {count}
            """).ToListAsync(ct);

        return rows.ConvertAll(row => new AnnouncementThread(PostId.From(row.PostId), ToDto(row)));
    }

    private static AnnouncementDto ToDto(AnnouncementRow row) =>
        new(AnnouncementId.From(row.Id), PostId.From(row.PostId), row.Message, row.CreatedAt, row.EditedAt);

    private sealed class AnnouncementRow
    {
        public int Id { get; init; }

        public int PostId { get; init; }

        public string Message { get; init; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset? EditedAt { get; init; }
    }
}
