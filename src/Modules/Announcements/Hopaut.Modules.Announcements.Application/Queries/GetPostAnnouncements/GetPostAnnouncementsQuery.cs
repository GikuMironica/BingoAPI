using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetPostAnnouncements;

/// <summary>
/// A post's announcements, newest first and keyset-paged by an opaque cursor, for the post's host, its accepted attendees
/// and administrators (decision D-033); inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record GetPostAnnouncementsQuery(PostId PostId, string? Cursor, int? Limit) : IQuery<Result<AnnouncementPage>>;
