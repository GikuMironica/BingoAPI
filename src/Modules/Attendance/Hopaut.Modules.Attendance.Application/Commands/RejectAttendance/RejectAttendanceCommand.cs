using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

public sealed record RejectAttendanceCommand(int PostId, string UserId, string OwnerId) : IRequest<bool>;
