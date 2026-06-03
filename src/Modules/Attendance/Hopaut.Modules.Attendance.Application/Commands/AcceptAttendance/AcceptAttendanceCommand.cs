using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed record AcceptAttendanceCommand(PostId PostId, UserId UserId, UserId OwnerId) : IRequest<bool>;
