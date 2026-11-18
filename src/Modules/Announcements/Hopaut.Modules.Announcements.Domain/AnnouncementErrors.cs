using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

/// <summary>Expected failures of the Announcement aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class AnnouncementErrors
{
    public static Error NotFound(AnnouncementId announcementId) =>
        Error.NotFound("announcements.announcement.not_found", $"Announcement {announcementId} was not found.");

    public static Error PostNotFound(PostId postId) =>
        Error.NotFound("announcements.announcement.post_not_found", $"Post {postId} was not found.");

    public static Error NotPostHost(PostId postId) =>
        Error.Forbidden("announcements.announcement.not_post_host", $"Only the host of post {postId} or an administrator can manage its announcements.");

    public static Error NotPostAudience(PostId postId) =>
        Error.Forbidden(
            "announcements.announcement.not_post_audience",
            $"Only the host of post {postId}, its accepted attendees or an administrator can read its announcements.");

    public static readonly Error MessageEmpty =
        Error.Validation("announcements.message.empty", "The message cannot be blank.");

    public static readonly Error MessageTooLong =
        Error.Validation("announcements.message.too_long", $"The message can have at most {AnnouncementMessage.MaxLength} characters.");
}
