using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

public sealed record RejectAttendanceCommand(PostId PostId, UserId UserId, UserId OwnerId) : IRequest<bool>;
