using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

/// <summary>Whether the caller's participation in the post is accepted.</summary>
public sealed record IsUserAttendingQuery(PostId PostId) : IQuery<Result<bool>>;
