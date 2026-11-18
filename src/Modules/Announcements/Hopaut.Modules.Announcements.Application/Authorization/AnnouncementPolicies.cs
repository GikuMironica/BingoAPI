namespace Hopaut.Modules.Announcements.Application.Authorization;

/// <summary>Resource-based policies of the Announcements module (plan Step 6.9), registered by <c>AddAnnouncementsModule</c>.</summary>
public static class AnnouncementPolicies
{
    /// <summary>
    /// The caller hosts the post the announcements belong to, or is an administrator (the legacy rule for creating, updating
    /// and deleting announcements). The resource is the post's <c>PostHost</c> from Posts.Contracts. Reading is open to
    /// the same callers and to the post's accepted attendees (decision D-033).
    /// </summary>
    public const string MustOwnAnnouncement = "announcements.post.host";
}
