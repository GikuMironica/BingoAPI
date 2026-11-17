using System.Globalization;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application.Queries;

/// <summary>Which of the caller's posts to list: active = listed and not ended; inactive = disabled or ended.</summary>
public enum HostedPostsFilter
{
    Active,
    Inactive,
}

/// <summary>One page of posts and the cursor of the next page (null on the last page), rest-api-standards §6.</summary>
public sealed record PostPage(IReadOnlyList<PostDto> Items, string? NextCursor);

/// <summary>Keyset position in a list ordered by start then ID, both descending: the last item already returned.</summary>
public sealed record PostKeyset(DateTimeOffset EventTime, PostId PostId)
{
    public Cursor Encode() =>
        Cursor.Encode(string.Create(CultureInfo.InvariantCulture, $"{EventTime.UtcTicks}:{PostId.Value}"));

    public static Result<PostKeyset> Decode(string? value)
    {
        var cursor = Cursor.Create(value);
        if (cursor.IsFailure)
            return cursor.Error;

        var parts = cursor.Value.Decode().Split(':');
        return parts.Length == 2
            && long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
            && ticks <= DateTimeOffset.MaxValue.UtcTicks
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            && id > 0
                ? new PostKeyset(new DateTimeOffset(ticks, TimeSpan.Zero), PostId.From(id))
                : PagingErrors.InvalidCursor;
    }
}

/// <summary>Post reads served by no-tracking projections, filtered and paged in SQL (never loading aggregates).</summary>
public interface IPostReadModel
{
    /// <summary>Up to <paramref name="count"/> posts of the host matching the filter, newest start first, after the given position.</summary>
    Task<IReadOnlyList<PostDto>> GetHostedAsync(
        UserId host, HostedPostsFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct);
}

public static class PostListErrors
{
    public static readonly Error UnknownStatus =
        Error.Validation("posts.post.unknown_list_status", "The status must be \"active\" or \"inactive\".");

    /// <summary>Parses the wire value of a list filter; absent means active.</summary>
    public static Result<HostedPostsFilter> ParseFilter(string? status) => status?.ToUpperInvariant() switch
    {
        null or "ACTIVE" => HostedPostsFilter.Active,
        "INACTIVE" => HostedPostsFilter.Inactive,
        _ => UnknownStatus,
    };
}
