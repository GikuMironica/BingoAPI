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
    // TODO: add index — CREATE INDEX IX_posts_UserId_EventTime_Id ON posts.posts ("UserId", "EventTime" DESC, "Id" DESC);
    public async Task<IReadOnlyList<PostDto>> GetHostedAsync(
        UserId host, HostedPostsFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct)
    {
        var active = filter == HostedPostsFilter.Active;
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
