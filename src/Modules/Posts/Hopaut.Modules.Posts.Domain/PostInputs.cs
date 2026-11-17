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

/// <summary>Where an event moves to: the point and its descriptive address parts.</summary>
public sealed record PostLocation(Coordinates Coordinates, PostPlace Place);

/// <summary>The host's edit of a post (the legacy update): every part is optional, and null keeps the current value.</summary>
public sealed record PostRevision
{
    public TimeRange? Schedule { get; init; }

    public PostLocation? Location { get; init; }

    public PostTitle? Title { get; init; }

    public PostDescription? Description { get; init; }

    public string? Requirements { get; init; }

    public Capacity? Slots { get; init; }

    public Money? EntranceFee { get; init; }

    public IReadOnlyCollection<TagId>? Tags { get; init; }
}
