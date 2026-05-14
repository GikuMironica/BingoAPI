using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed record AcceptAttendanceCommand(int PostId, string UserId, string OwnerId) : IRequest<bool>;
