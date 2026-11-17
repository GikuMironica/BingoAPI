using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.CreatePost;

public sealed class CreatePostCommandHandler(IPostRepository posts, TimeProvider timeProvider)
    : ICommandHandler<CreatePostCommand, Result<PostId>>
{
    public async ValueTask<Result<PostId>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var schedule = TimeRange.Create(request.EventTime, request.EndTime);
        var coordinates = Coordinates.Create(request.Location.Latitude, request.Location.Longitude);
        var title = PostTitle.Create(request.Event.Title);
        var description = PostDescription.Create(request.Event.Description);
        var slots = request.Event.Slots is { } count ? Capacity.Create(count) : null;
        var entranceFee = LegacyCurrencies.EntranceFee(request.Event.EntrancePrice, request.Event.Currency);

        var invalid = ValidationError.FromFailures(
            ("endTime", schedule),
            ("location", coordinates),
            ("event.title", title),
            ("event.description", description),
            ("event.slots", slots),
            ("event.entrancePrice", entranceFee));
        if (invalid is not null)
            return invalid;

        var id = await posts.NextIdAsync(cancellationToken);
        var created = Post.Create(
            id,
            request.UserId,
            schedule.Value,
            coordinates.Value,
            new PostPlace(request.Location.EntityName, request.Location.City, request.Location.Region, request.Location.Address, request.Location.Country),
            new EventDetails(
                request.Event.EventType,
                title.Value,
                description.Value,
                request.Event.Requirements,
                slots?.Value,
                entranceFee?.Value,
                request.Event.TypeSpecificData),
            request.PictureUrls,
            timeProvider.GetUtcNow());
        if (created.IsFailure)
            return created.Error;

        posts.Add(created.Value);
        return created.Value.Id;
    }
}
