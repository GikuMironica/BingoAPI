using MediatR;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed record IsUserAttendingQuery(int PostId, string UserId) : IRequest<bool>;
