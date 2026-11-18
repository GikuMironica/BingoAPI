namespace Hopaut.Modules.Posts.Api.Contracts.V1;

/// <summary>
/// Body of <c>PATCH /api/v1/posts/{postId}</c>: a partial edit where an omitted (null) part keeps its value and a location
/// replaces the whole location. Times are ISO-8601; the fee keeps today's price + legacy currency index (D-026) until the
/// Posts contract is synchronised in plan Step 10.4.
/// </summary>
public sealed record UpdatePostRequest(
    DateTimeOffset? EventTime,
    DateTimeOffset? EndTime,
    UpdatePostLocationRequest? Location,
    UpdatePostEventRequest? Event,
    IReadOnlyList<string>? TagNames);

public sealed record UpdatePostLocationRequest(
    string? EntityName, string? City, string? Region, string? Address, string? Country, double Longitude, double Latitude);

public sealed record UpdatePostEventRequest(
    string? Title, string? Description, string? Requirements, int? Slots, double? EntrancePrice, int? Currency);
