using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed record RequestAttendanceCommand(PostId PostId, UserId UserId) : IRequest<bool>;
