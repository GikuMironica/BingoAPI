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

    /// <summary>Applies the given parts of a revision (null keeps a value); <c>Notify</c> when attendees should hear about it (requirements or fee changed).</summary>
    internal (bool Changed, bool Notify) Revise(PostRevision revision)
    {
        var notify = (revision.Requirements is not null && revision.Requirements != Requirements)
            || (revision.EntranceFee is not null && revision.EntranceFee != EntranceFee);
        var changed = notify
            || (revision.Title is not null && revision.Title != Title)
            || (revision.Description is not null && revision.Description != Description)
            || (revision.Slots is not null && revision.Slots != Slots);

        Title = revision.Title ?? Title;
        Description = revision.Description ?? Description;
        Requirements = revision.Requirements ?? Requirements;
        Slots = revision.Slots ?? Slots;
        EntranceFee = revision.EntranceFee ?? EntranceFee;
        return (changed, notify);
    }
}
