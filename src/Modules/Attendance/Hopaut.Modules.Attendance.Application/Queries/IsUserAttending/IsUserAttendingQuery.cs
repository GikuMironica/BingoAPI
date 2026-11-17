using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed record IsUserAttendingQuery(PostId PostId, UserId UserId) : IQuery<Result<bool>>;
