using Hopaut.Modules.Posts.Domain;

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
    public static PostDto From(Post post, UserReputationInfo? reputation = null) => new(
        post.Id.Value,
        post.PostTime,
        post.Schedule.Start,
        post.Schedule.End,
        (int)post.Status,
        post.UserId.Value,
        new EventLocationDto(
            post.Location.EntityName,
            post.Location.City,
            post.Location.Region,
            post.Location.Address,
            post.Location.Country,
            post.Location.Location.X,
            post.Location.Location.Y),
        new EventDto(
            post.Event.EventType,
            (double?)post.Event.EntranceFee?.Amount,
            post.Event.EntranceFee is { } fee ? LegacyCurrencies.IndexOf(fee.Currency) : null,
            post.Event.Title.Value,
            post.Event.Description.Value,
            post.Event.Requirements,
            post.Event.Slots?.Value),
        post.Pictures.Select(picture => new PictureDto(picture.Id.Value, picture.Url, picture.State.ToString())).ToList(),
        post.Tags.Select(tag => tag.Tag!.Name.Value).ToList(),
        reputation);
}

public sealed record PictureDto(int Id, string Url, string State);

public sealed record EventLocationDto(string? EntityName, string? City, string? Region, string? Address, string? Country, double Longitude, double Latitude);

public sealed record EventDto(EventType EventType, double? EntrancePrice, int? Currency, string? Title, string Description, string? Requirements, int? Slots);
