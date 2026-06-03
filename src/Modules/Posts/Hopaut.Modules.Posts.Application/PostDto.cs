using Hopaut.Modules.Posts.Domain;

namespace Hopaut.Modules.Posts.Application;

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
    UserReputationInfo? UserReputation = null);

public sealed record PictureDto(int Id, string Url, string State);

public sealed record EventLocationDto(string? EntityName, string? City, string? Region, string? Address, string? Country, double Longitude, double Latitude);

public sealed record EventDto(EventType EventType, double? EntrancePrice, int? Currency, string? Title, string Description, string? Requirements, int? Slots);
