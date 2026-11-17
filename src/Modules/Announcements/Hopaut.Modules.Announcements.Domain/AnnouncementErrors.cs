using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

/// <summary>Expected failures of the Announcement aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class AnnouncementErrors
{
    public static Error NotFound(AnnouncementId announcementId) =>
        Error.NotFound("announcements.announcement.not_found", $"Announcement {announcementId} was not found.");
}
