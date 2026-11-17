using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetMyAttendances;

/// <summary>
/// The posts the caller attends (accepted participations), newest start first: <c>active</c> (default) or <c>inactive</c>,
/// keyset-paged by an opaque cursor; inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record GetMyAttendancesQuery(string? Status, string? Cursor, int? Limit) : IQuery<Result<PostPage>>;
