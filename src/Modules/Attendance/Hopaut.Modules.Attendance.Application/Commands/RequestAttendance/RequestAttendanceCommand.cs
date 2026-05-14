using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed record RequestAttendanceCommand(int PostId, string UserId) : IRequest<bool>;
