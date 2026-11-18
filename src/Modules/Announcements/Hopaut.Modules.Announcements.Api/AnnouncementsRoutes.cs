namespace Hopaut.Modules.Announcements.Api;

/// <summary>Route templates of the Announcements endpoints (rest-api-standards §3: no inline literals).</summary>
public static class AnnouncementsRoutes
{
    /// <summary>Group of today's routes (plan Step 10.6 moves creation under the post).</summary>
    public const string Announcements = "/api/v1/announcements";

    /// <summary>Under <see cref="Announcements"/>: POST creates.</summary>
    public const string Create = "/";

    /// <summary>Under <see cref="Announcements"/>: GET, PUT and DELETE one announcement.</summary>
    public const string ById = "/{announcementId:int}";

    /// <summary>Under <see cref="Announcements"/>: today's unpaged list of a post's announcements (removed in plan Step 10.6).</summary>
    public const string ByPost = "/post/{postId:int}";

    /// <summary>A post's announcements, cursor-paged.</summary>
    public const string PostAnnouncements = "/api/v1/posts/{postId:int}/announcements";

    /// <summary>The caller's inbox or outbox threads, cursor-paged.</summary>
    public const string MyThreads = "/api/v1/users/me/announcement-threads";

    /// <summary>The location of one announcement.</summary>
    public static string Announcement(int announcementId) => $"{Announcements}/{announcementId}";
}
