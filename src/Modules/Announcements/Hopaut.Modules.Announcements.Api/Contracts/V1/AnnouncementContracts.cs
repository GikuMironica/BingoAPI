namespace Hopaut.Modules.Announcements.Api.Contracts.V1;

/// <summary>The new message of an announcement: trimmed, 1–500 characters.</summary>
public sealed record UpdateAnnouncementRequest(string? Message);

/// <summary>One announcement; times are ISO-8601 UTC and <c>editedAt</c> is null until the message is edited.</summary>
public sealed record AnnouncementResponse(int Id, int PostId, string Message, DateTimeOffset CreatedAt, DateTimeOffset? EditedAt);

/// <summary>One page of a post's announcements, newest first; <c>nextCursor</c> is null on the last page.</summary>
public sealed record AnnouncementPageResponse(IReadOnlyList<AnnouncementResponse> Items, string? NextCursor);

/// <summary>A post with announcements and its newest announcement.</summary>
public sealed record AnnouncementThreadResponse(int PostId, AnnouncementResponse LastAnnouncement);

/// <summary>One page of threads, the most recently announced first; <c>nextCursor</c> is null on the last page.</summary>
public sealed record AnnouncementThreadPageResponse(IReadOnlyList<AnnouncementThreadResponse> Items, string? NextCursor);
