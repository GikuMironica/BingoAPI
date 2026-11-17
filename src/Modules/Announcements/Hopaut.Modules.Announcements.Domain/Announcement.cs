using Hopaut.Modules.Announcements.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

public sealed class Announcement : AggregateRoot<AnnouncementId>
{
    public PostId PostId { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Announcement() { } // EF Core

    /// <summary>Creates an announcement with an ID taken from the announcements sequence, so the created event carries it.</summary>
    public static Announcement Create(AnnouncementId id, PostId postId, string message, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);

        var announcement = new Announcement
        {
            Id = id,
            PostId = postId,
            Message = message,
            CreatedAt = now
        };

        announcement.Raise(new AnnouncementCreatedDomainEvent(now, id, postId));

        return announcement;
    }
}
