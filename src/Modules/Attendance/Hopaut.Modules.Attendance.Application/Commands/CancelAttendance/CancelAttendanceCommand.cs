using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed record CancelAttendanceCommand(PostId PostId, UserId UserId) : IRequest<bool>;
