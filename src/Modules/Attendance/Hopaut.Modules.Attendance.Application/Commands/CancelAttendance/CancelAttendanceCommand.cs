using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed record CancelAttendanceCommand(int PostId, string UserId) : IRequest<bool>;
