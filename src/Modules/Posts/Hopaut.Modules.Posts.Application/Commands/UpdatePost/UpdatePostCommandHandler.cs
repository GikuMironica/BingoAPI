using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Application.Authorization;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.UpdatePost;

public sealed class UpdatePostCommandHandler(
    IPostRepository posts,
    ITagRepository tags,
    IAttendanceQueries attendance,
    IResourceAuthorizer authorizer,
    TimeProvider timeProvider) : ICommandHandler<UpdatePostCommand, Result>
{
    public async ValueTask<Result> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await posts.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return PostErrors.NotFound(request.PostId);

        if (!await authorizer.IsAuthorizedAsync(post, PostPolicies.MustOwnPost, cancellationToken))
            return PostErrors.NotHost(request.PostId);

        var schedule = request.EventTime is null && request.EndTime is null
            ? null
            : TimeRange.Create(request.EventTime ?? post.Schedule.Start, request.EndTime ?? post.Schedule.End);
        var coordinates = request.Location is { } location ? Coordinates.Create(location.Latitude, location.Longitude) : null;
        var title = request.Event?.Title is { } newTitle ? PostTitle.Create(newTitle) : null;
        var description = request.Event?.Description is { } newDescription ? PostDescription.Create(newDescription) : null;
        var slots = request.Event?.Slots is { } count ? Capacity.Create(count) : null;
        var entranceFee = EntranceFee(request.Event, post.Event.EntranceFee);
        var tagNames = request.TagNames is null ? null : TagName.CreateMany(request.TagNames);

        var invalid = ValidationError.FromFailures(
            ("endTime", schedule),
            ("location", coordinates),
            ("event.title", title),
            ("event.description", description),
            ("event.slots", slots),
            ("event.entrancePrice", entranceFee),
            ("tagNames", tagNames));
        if (invalid is not null)
            return invalid;

        var tagIds = tagNames is null
            ? null
            : (await tags.GetOrCreateAsync(tagNames.Value, cancellationToken)).Select(tag => tag.Id).ToList();
        var acceptedGuests = slots is null ? 0 : await attendance.CountAcceptedAsync(post.Id, cancellationToken);

        var revision = new PostRevision
        {
            Schedule = schedule?.Value,
            Location = request.Location is { } place && coordinates is { IsSuccess: true } point
                ? new PostLocation(point.Value, new PostPlace(place.EntityName, place.City, place.Region, place.Address, place.Country))
                : null,
            Title = title?.Value,
            Description = description?.Value,
            Requirements = request.Event?.Requirements,
            Slots = slots?.Value,
            EntranceFee = entranceFee?.Value,
            Tags = tagIds,
        };
        return post.Update(revision, acceptedGuests, timeProvider.GetUtcNow());
    }

    /// <summary>A new price or currency changes the fee; the part not sent is taken from the current fee.</summary>
    private static Result<Money>? EntranceFee(UpdatePostEventInput? input, Money? current)
    {
        if (input is null || (input.EntrancePrice is null && input.Currency is null))
            return null;

        return LegacyCurrencies.EntranceFee(
            input.EntrancePrice ?? (double?)current?.Amount,
            input.Currency ?? (current is null ? null : LegacyCurrencies.IndexOf(current.Currency)));
    }
}
