using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementThreads;

/// <summary>
/// The caller's announcement threads (decision D-033): <c>inbox</c> (default) lists the posts the caller attends as an
/// accepted attendee, <c>outbox</c> the posts the caller hosts; only posts with announcements, the most recently announced
/// first, keyset-paged by an opaque cursor. Inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record GetAnnouncementThreadsQuery(string? Box, string? Cursor, int? Limit) : IQuery<Result<AnnouncementThreadPage>>;
