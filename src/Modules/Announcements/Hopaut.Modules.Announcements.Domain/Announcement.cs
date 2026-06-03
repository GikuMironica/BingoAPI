using Hopaut.Modules.Announcements.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

public sealed class Announcement : AggregateRoot<AnnouncementId>
{
    public PostId PostId { get; private set; }
    public string Message { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Announcement() { } // EF Core

    public static Announcement Create(PostId postId, string message)
    {
        var announcement = new Announcement
        {
            PostId = postId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow
        };

        announcement.AddDomainEvent(new AnnouncementCreatedDomainEvent(announcement.Id, postId));

        return announcement;
    }
}
