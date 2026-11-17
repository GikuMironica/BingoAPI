using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// What a post's event offers (one row per post; the former nine-class hierarchy collapsed into <see cref="EventType"/>
/// plus JSONB <see cref="TypeSpecificData"/>). Changes only through <see cref="Post"/>.
/// </summary>
public sealed class Event
{
    private Event() { } // EF Core

    public int Id { get; private set; }

    public PostId PostId { get; private set; }

    public EventType EventType { get; private set; }

    public PostTitle Title { get; private set; }

    public PostDescription Description { get; private set; }

    public string? Requirements { get; private set; }

    public Capacity? Slots { get; private set; }

    public Money? EntranceFee { get; private set; }

    public string? TypeSpecificData { get; private set; }

    internal static Event Create(PostId postId, EventDetails details) => new()
    {
        PostId = postId,
        EventType = details.Type,
        Title = details.Title,
        Description = details.Description,
        Requirements = details.Requirements,
        Slots = details.Slots,
        EntranceFee = details.EntranceFee,
        TypeSpecificData = details.TypeSpecificData,
    };
}
