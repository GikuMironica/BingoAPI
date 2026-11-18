using System.Globalization;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application.Queries;

/// <summary>Which posts of a list to show: active = listed and not ended; inactive = disabled or ended (the legacy lists).</summary>
public enum PostListFilter
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

/// <summary>Keyset position in a feed ordered by distance then ID, both ascending: the last item already returned.</summary>
public sealed record NearbyKeyset(double DistanceMeters, PostId PostId)
{
    public Cursor Encode() =>
        Cursor.Encode(string.Create(CultureInfo.InvariantCulture, $"{DistanceMeters:R}:{PostId.Value}"));

    public static Result<NearbyKeyset> Decode(string? value)
    {
        var cursor = Cursor.Create(value);
        if (cursor.IsFailure)
            return cursor.Error;

        var parts = cursor.Value.Decode().Split(':');
        return parts.Length == 2
            && double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var distance)
            && double.IsFinite(distance)
            && distance >= 0
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            && id > 0
                ? new NearbyKeyset(distance, PostId.From(id))
                : PagingErrors.InvalidCursor;
    }
}

/// <summary>A post of the feed and its distance from the searched point.</summary>
public sealed record NearbyPost(PostDto Post, double DistanceMeters);

/// <summary>Post reads served by no-tracking projections, filtered and paged in SQL (never loading aggregates).</summary>
public interface IPostReadModel
{
    Task<PostDto?> GetByIdAsync(PostId postId, CancellationToken ct);

    /// <summary>Up to <paramref name="count"/> active, not yet ended posts within the radius, nearest first, after the given position.</summary>
    Task<IReadOnlyList<NearbyPost>> GetNearbyAsync(
        Coordinates center, RadiusMeters radius, DateTimeOffset now, NearbyKeyset? after, int count, CancellationToken ct);

    /// <summary>Up to <paramref name="count"/> posts of the host matching the filter, newest start first, after the given position.</summary>
    Task<IReadOnlyList<PostDto>> GetHostedAsync(
        UserId host, PostListFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct);

    /// <summary>Up to <paramref name="count"/> of the given posts matching the filter, newest start first, after the given position.</summary>
    Task<IReadOnlyList<PostDto>> GetAttendedAsync(
        IReadOnlyCollection<PostId> attended, PostListFilter filter, DateTimeOffset now, PostKeyset? after, int count, CancellationToken ct);
}

public static class PostListErrors
{
    public static readonly Error UnknownStatus =
        Error.Validation("posts.post.unknown_list_status", "The status must be \"active\" or \"inactive\".");

    /// <summary>Parses the wire value of a list filter; absent means active.</summary>
    public static Result<PostListFilter> ParseFilter(string? status) => status?.ToUpperInvariant() switch
    {
        null or "ACTIVE" => PostListFilter.Active,
        "INACTIVE" => PostListFilter.Inactive,
        _ => UnknownStatus,
    };
}
