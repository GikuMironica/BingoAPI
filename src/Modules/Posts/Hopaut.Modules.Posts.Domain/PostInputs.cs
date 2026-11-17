using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>The descriptive part of a post's location; every part is optional, as in the legacy API.</summary>
public sealed record PostPlace(string? EntityName, string? City, string? Region, string? Address, string? Country);

/// <summary>What an event offers, built by the Posts handlers from validated value objects.</summary>
public sealed record EventDetails(
    EventType Type,
    PostTitle Title,
    PostDescription Description,
    string? Requirements,
    Capacity? Slots,
    Money? EntranceFee,
    string? TypeSpecificData);
