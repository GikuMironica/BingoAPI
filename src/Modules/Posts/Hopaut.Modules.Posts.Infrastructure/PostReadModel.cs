using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Application.Queries;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>
/// Serves <see cref="IPostReadModel"/> in two no-tracking steps: one SQL query filters, orders and pages the keys
/// (a keyset row comparison, which LINQ cannot express on value-converted IDs and EF Core cannot compose over
/// <c>FromSql</c> with complex-type columns), then one projection loads those posts' parts; tag names are joined by
/// <see cref="TagId"/>.
/// </summary>
public sealed class PostReadModel(PostsModuleDbContext db) : IPostReadModel
{
    public async Task<PostDto?> GetByIdAsync(PostId postId, CancellationToken ct) =>
        (await LoadAsync([postId], ct)).SingleOrDefault();

    // The GIST index on event_locations."Location" serves ST_DWithin; distances are meters on the geography type.
    public async Task<IReadOnlyList<NearbyPost>> GetNearbyAsync(
        Coordinates center, RadiusMeters radius, DateTimeOffset now, NearbyKeyset? after, int count, CancellationToken ct)
    {
        var keys = await db.Database.SqlQuery<NearbyKey>($"""
            SELECT nearby."Id", nearby."Distance"
            FROM (
                SELECT p."Id",
                       ST_Distance(l."Location", ST_SetSRID(ST_MakePoint({center.Longitude}, {center.Latitude}), 4326)::geography) AS "Distance"
                FROM posts.posts AS p
                JOIN posts.event_locations AS l ON l."PostId" = p."Id"
                WHERE p."Status" = {(int)PostStatus.Active}
                  AND p."EndTime" > {now}
                  AND ST_DWithin(l."Location", ST_SetSRID(ST_MakePoint({center.Longitude}, {center.Latitude}), 4326)::geography, {radius.Value})
            ) AS nearby
            WHERE {after is null} OR (nearby."Distance", nearby."Id") > ({after?.DistanceMeters ?? 0}, {after?.PostId.Value ?? 0})
            ORDER BY nearby."Distance", nearby."Id"
            LIMIT {count}
            """).ToListAsync(ct);

        var distances = keys.ToDictionary(key => key.Id, key => key.Distance);
        var posts = await LoadAsync(keys.ConvertAll(key => PostId.From(key.Id)), ct);
        return posts.Select(post => new NearbyPost(post, distances[post.Id])).ToList();
    }

    // TODO: add index — CREATE INDEX IX_posts_UserId_EventTime_Id ON posts.posts ("UserId", "EventTime" DESC, "Id" DESC);
    public async Task<IReadOnlyList<PostDto>> GetHostedAsync(
        UserId host, PostListFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct)
    {
        var active = filter == PostListFilter.Active;
        var keys = await db.Database.SqlQuery<PostKey>($"""
            SELECT p."Id", p."EventTime"
            FROM posts.posts AS p
            WHERE p."UserId" = {host.Value}
              AND CASE WHEN {active}
                       THEN p."Status" = {(int)PostStatus.Active} AND p."EndTime" > {now}
                       ELSE p."Status" = {(int)PostStatus.Disabled} OR p."EndTime" <= {now}
                  END
              AND ({after is null} OR (p."EventTime", p."Id") < ({after?.EventTime ?? now}, {after?.PostId.Value ?? 0}))
            ORDER BY p."EventTime" DESC, p."Id" DESC
            LIMIT {count}
            """).ToListAsync(ct);

        return await LoadAsync(keys.ConvertAll(key => PostId.From(key.Id)), ct);
    }

    // The primary key serves the ID filter; the list is bounded by the caller's accepted participations.
    public async Task<IReadOnlyList<PostDto>> GetAttendedAsync(
        IReadOnlyCollection<PostId> attended, PostListFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct)
    {
        if (attended.Count == 0)
            return [];

        var ids = attended.Select(id => id.Value).ToArray();
        var active = filter == PostListFilter.Active;
        var keys = await db.Database.SqlQuery<PostKey>($"""
            SELECT p."Id", p."EventTime"
            FROM posts.posts AS p
            WHERE p."Id" = ANY({ids})
              AND CASE WHEN {active}
                       THEN p."Status" = {(int)PostStatus.Active} AND p."EndTime" > {now}
                       ELSE p."Status" = {(int)PostStatus.Disabled} OR p."EndTime" <= {now}
                  END
              AND ({after is null} OR (p."EventTime", p."Id") < ({after?.EventTime ?? now}, {after?.PostId.Value ?? 0}))
            ORDER BY p."EventTime" DESC, p."Id" DESC
            LIMIT {count}
            """).ToListAsync(ct);

        return await LoadAsync(keys.ConvertAll(key => PostId.From(key.Id)), ct);
    }

    /// <summary>The posts with these IDs, in the given order.</summary>
    private async Task<IReadOnlyList<PostDto>> LoadAsync(List<PostId> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
            return [];

        var rows = await db.Posts
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new PostRow(
                p.Id,
                p.PostTime,
                p.Schedule.Start,
                p.Schedule.End,
                p.Status,
                p.UserId,
                p.Location,
                p.Event,
                p.Pictures.ToList(),
                p.Tags.Select(link => link.TagId).ToList()))
            .AsSplitQuery()
            .ToListAsync(ct);

        var tagIds = rows.SelectMany(row => row.TagIds).Distinct().ToList();
        var tagNames = tagIds.Count == 0
            ? []
            : await db.Tags.AsNoTracking().Where(tag => tagIds.Contains(tag.Id)).ToDictionaryAsync(tag => tag.Id, tag => tag.Name.Value, ct);

        var byId = rows.ToDictionary(row => row.Id);
        return ids
            .Where(byId.ContainsKey)
            .Select(id => byId[id])
            .Select(row => PostDto.Create(
                row.Id,
                row.PostTime,
                row.EventTime,
                row.EndTime,
                row.Status,
                row.Host,
                row.Location,
                row.Event,
                row.Pictures,
                row.TagIds.Select(tagId => tagNames[tagId]).ToList()))
            .ToList();
    }

    private sealed class PostKey
    {
        public int Id { get; init; }

        public DateTimeOffset EventTime { get; init; }
    }

    private sealed class NearbyKey
    {
        public int Id { get; init; }

        public double Distance { get; init; }
    }

    private sealed record PostRow(
        PostId Id,
        DateTimeOffset PostTime,
        DateTimeOffset EventTime,
        DateTimeOffset? EndTime,
        PostStatus Status,
        UserId Host,
        EventLocation Location,
        Event Event,
        List<Picture> Pictures,
        List<TagId> TagIds);
}
