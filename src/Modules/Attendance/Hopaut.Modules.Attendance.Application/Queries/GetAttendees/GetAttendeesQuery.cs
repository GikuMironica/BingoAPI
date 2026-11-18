using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.GetAttendees;

/// <summary>
/// The attendees of a post, earliest request first: <c>accepted</c> (default; visible to every signed-in user, like the
/// legacy list) or <c>pending</c> (the host only), keyset-paged by an opaque cursor; inputs arrive as sent and the handler
/// validates them.
/// </summary>
public sealed record GetAttendeesQuery(PostId PostId, string? Status, string? Cursor, int? Limit) : IQuery<Result<AttendeePage>>;
