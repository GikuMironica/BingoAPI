using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

public sealed class GetNearbyPostsQueryHandler(IPostReadModel readModel, IUserReputationProvider reputationProvider, TimeProvider timeProvider)
    : IQueryHandler<GetNearbyPostsQuery, Result<PostPage>>
{
    public async ValueTask<Result<PostPage>> Handle(GetNearbyPostsQuery request, CancellationToken cancellationToken)
    {
        var center = Coordinates.Create(request.Latitude, request.Longitude);
        var radius = request.RadiusMeters is { } meters ? Radius(meters) : Result.Success(RadiusMeters.Default);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);
        var after = request.Cursor is null ? null : NearbyKeyset.Decode(request.Cursor);

        var invalid = ValidationError.FromFailures(("location", center), ("radius", radius), ("limit", pageSize), ("cursor", after));
        if (invalid is not null)
            return invalid;

        var nearby = await readModel.GetNearbyAsync(
            center.Value, radius.Value, timeProvider.GetUtcNow(), after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var page = nearby.Take(pageSize.Value.Value).ToList();
        var nextCursor = nearby.Count > page.Count
            ? new NearbyKeyset(page[^1].DistanceMeters, PostId.From(page[^1].Post.Id)).Encode().Value
            : null;

        // One batched reputation lookup for all hosts (no N+1).
        var hosts = page.Select(item => UserId.From(item.Post.UserId)).Distinct().ToList();
        var reputations = hosts.Count == 0 ? [] : await reputationProvider.GetBatchAsync(hosts, cancellationToken);

        var items = page.ConvertAll(item => item.Post with { UserReputation = reputations.GetValueOrDefault(UserId.From(item.Post.UserId)) });
        return new PostPage(items, nextCursor);
    }

    /// <summary>Whole meters within the feed bounds; anything else is out of range.</summary>
    private static Result<RadiusMeters> Radius(double meters) =>
        double.IsFinite(meters) && meters is >= RadiusMeters.Min and <= RadiusMeters.Max
            ? RadiusMeters.Create((int)Math.Round(meters))
            : RadiusMetersErrors.OutOfRange;
}
