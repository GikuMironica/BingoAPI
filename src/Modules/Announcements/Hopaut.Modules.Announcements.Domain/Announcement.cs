using Hopaut.Modules.Announcements.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

/// <summary>
/// A message the host of a post addresses to the post's attendees (rules recovered from the legacy API, decision D-033).
/// The post's host or an administrator announces, edits and deletes; the host, the post's accepted attendees and
/// administrators read. An edit replaces the message and records when it happened.
/// </summary>
public sealed class Announcement : AggregateRoot<AnnouncementId>
{
    private Announcement() { } // EF Core

    public PostId PostId { get; private set; }

    public AnnouncementMessage Message { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the message was last changed; null while it is the message the announcement was created with.</summary>
    public DateTimeOffset? EditedAt { get; private set; }

    /// <summary>Optimistic-concurrency token (PostgreSQL <c>xmin</c>): the host and administrators change an announcement independently.</summary>
    public uint Version { get; private set; }

    /// <summary>Announces on the post with an ID taken from the announcements sequence, so the created event carries it.</summary>
    public static Announcement Create(AnnouncementId id, PostId postId, AnnouncementMessage message, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(message);

        var announcement = new Announcement
        {
            Id = id,
            PostId = postId,
            Message = message,
            CreatedAt = now,
        };

        announcement.Raise(new AnnouncementCreatedDomainEvent(now, id, postId));
        return announcement;
    }

    /// <summary>Replaces the message. Sending the current message again changes nothing, so it is not an edit.</summary>
    public void Edit(AnnouncementMessage message, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message == Message)
            return;

        Message = message;
        EditedAt = now;
        Raise(new AnnouncementEditedDomainEvent(now, Id, PostId));
    }

    /// <summary>Marks the announcement as deleted; the repository removes it in the same unit of work.</summary>
    public void Delete(DateTimeOffset now) => Raise(new AnnouncementDeletedDomainEvent(now, Id, PostId));
}
