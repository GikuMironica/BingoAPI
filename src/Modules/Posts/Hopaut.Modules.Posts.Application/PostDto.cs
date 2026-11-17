using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application;

/// <summary>Today's wire shape of a post (unsynchronised until plan Step 10.4, D-007): value objects map back to primitives.</summary>
public sealed record PostDto(
    int Id,
    DateTimeOffset PostTime,
    DateTimeOffset EventTime,
    DateTimeOffset? EndTime,
    int ActiveFlag,
    string UserId,
    EventLocationDto Location,
    EventDto Event,
    IReadOnlyList<PictureDto> Pictures,
    IReadOnlyList<string> Tags,
    UserReputationInfo? UserReputation = null)
{
    public static PostDto From(Post post, UserReputationInfo? reputation = null) => Create(
        post.Id,
        post.PostTime,
        post.Schedule.Start,
        post.Schedule.End,
        post.Status,
        post.UserId,
        post.Location,
        post.Event,
        post.Pictures,
        post.Tags.Select(tag => tag.Tag!.Name.Value).ToList(),
        reputation);

    /// <summary>Maps the parts a read model projects (the aggregate's children, read without tracking) to the wire shape.</summary>
    public static PostDto Create(
        PostId id,
        DateTimeOffset postTime,
        DateTimeOffset eventTime,
        DateTimeOffset? endTime,
        PostStatus status,
        UserId host,
        EventLocation location,
        Event @event,
        IEnumerable<Picture> pictures,
        IReadOnlyList<string> tags,
        UserReputationInfo? reputation = null) => new(
        id.Value,
        postTime,
        eventTime,
        endTime,
        (int)status,
        host.Value,
        new EventLocationDto(
            location.EntityName,
            location.City,
            location.Region,
            location.Address,
            location.Country,
            location.Location.X,
            location.Location.Y),
        new EventDto(
            @event.EventType,
            (double?)@event.EntranceFee?.Amount,
            @event.EntranceFee is { } fee ? LegacyCurrencies.IndexOf(fee.Currency) : null,
            @event.Title.Value,
            @event.Description.Value,
            @event.Requirements,
            @event.Slots?.Value),
        pictures.Select(picture => new PictureDto(picture.Id.Value, picture.Url, picture.State.ToString())).ToList(),
        tags,
        reputation);
}

public sealed record PictureDto(int Id, string Url, string State);

public sealed record EventLocationDto(string? EntityName, string? City, string? Region, string? Address, string? Country, double Longitude, double Latitude);

public sealed record EventDto(EventType EventType, double? EntrancePrice, int? Currency, string? Title, string Description, string? Requirements, int? Slots);
