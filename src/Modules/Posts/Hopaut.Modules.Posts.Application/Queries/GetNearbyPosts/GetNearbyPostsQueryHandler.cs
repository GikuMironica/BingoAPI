using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

public sealed class GetNearbyPostsQueryHandler(IPostRepository posts, IUserReputationProvider reputationProvider)
    : IQueryHandler<GetNearbyPostsQuery, Result<IReadOnlyList<PostDto>>>
{
    public async ValueTask<Result<IReadOnlyList<PostDto>>> Handle(GetNearbyPostsQuery request, CancellationToken cancellationToken)
    {
        var nearby = await posts.GetNearbyAsync(request.Longitude, request.Latitude, request.RadiusMeters, request.Limit, cancellationToken);

        // One batched reputation lookup for all hosts (no N+1).
        var hosts = nearby.Select(p => p.UserId).Distinct().ToList();
        var reputations = await reputationProvider.GetBatchAsync(hosts, cancellationToken);

        IReadOnlyList<PostDto> dtos = nearby.Select(p => PostDto.From(p, reputations.GetValueOrDefault(p.UserId))).ToList();
        return Result.Success(dtos);
    }
}
