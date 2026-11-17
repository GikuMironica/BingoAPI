using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

public sealed class GetNearbyPostsQueryHandler(IPostRepository posts, IUserReputationProvider reputationProvider)
    : IQueryHandler<GetNearbyPostsQuery, Result<IReadOnlyList<PostDto>>>
{
    public async ValueTask<Result<IReadOnlyList<PostDto>>> Handle(GetNearbyPostsQuery request, CancellationToken cancellationToken)
    {
        var nearby = await posts.GetNearbyAsync(request.Longitude, request.Latitude, request.RadiusMeters, request.Limit, cancellationToken);

        // Batch-fetch reputations for all distinct user IDs in one query (kills N+1)
        var userIds = nearby.Select(p => p.UserId).Distinct().ToList();
        var reputations = await reputationProvider.GetBatchAsync(userIds, cancellationToken);

        IReadOnlyList<PostDto> dtos = nearby.Select(p => new PostDto(
            p.Id.Value, p.PostTime, p.EventTime, p.EndTime, p.ActiveFlag, p.UserId.Value,
            new EventLocationDto(p.Location.EntityName, p.Location.City, p.Location.Region, p.Location.Address, p.Location.Country, p.Location.Location.X, p.Location.Location.Y),
            new EventDto(p.Event.EventType, p.Event.EntrancePrice, p.Event.Currency, p.Event.Title, p.Event.Description, p.Event.Requirements, p.Event.Slots),
            p.Pictures.Select(pic => new PictureDto(pic.Id.Value, pic.Url, pic.State.ToString())).ToList(),
            p.Tags.Select(t => t.Tag!.TagName).ToList(),
            reputations.GetValueOrDefault(p.UserId)
        )).ToList();

        return Result.Success(dtos);
    }
}
