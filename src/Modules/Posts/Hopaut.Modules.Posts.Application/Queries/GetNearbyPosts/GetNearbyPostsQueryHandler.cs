using MediatR;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

public sealed class GetNearbyPostsQueryHandler : IRequestHandler<GetNearbyPostsQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostRepository _repo;
    private readonly IUserReputationProvider _reputationProvider;

    public GetNearbyPostsQueryHandler(IPostRepository repo, IUserReputationProvider reputationProvider)
    {
        _repo = repo;
        _reputationProvider = reputationProvider;
    }

    public async Task<IReadOnlyList<PostDto>> Handle(GetNearbyPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await _repo.GetNearbyAsync(request.Longitude, request.Latitude, request.RadiusMeters, request.Limit, cancellationToken);

        // Batch-fetch reputations for all distinct user IDs in one query (kills N+1)
        var userIds = posts.Select(p => p.UserId).Distinct().ToList();
        var reputations = await _reputationProvider.GetBatchAsync(userIds, cancellationToken);

        return posts.Select(p => new PostDto(
            p.Id.Value, p.PostTime, p.EventTime, p.EndTime, p.ActiveFlag, p.UserId.Value,
            new EventLocationDto(p.Location.EntityName, p.Location.City, p.Location.Region, p.Location.Address, p.Location.Country, p.Location.Location.X, p.Location.Location.Y),
            new EventDto(p.Event.EventType, p.Event.EntrancePrice, p.Event.Currency, p.Event.Title, p.Event.Description, p.Event.Requirements, p.Event.Slots),
            p.Pictures.Select(pic => new PictureDto(pic.Id.Value, pic.Url, pic.State.ToString())).ToList(),
            p.Tags.Select(t => t.Tag!.TagName).ToList(),
            reputations.GetValueOrDefault(p.UserId)
        )).ToList();
    }
}
