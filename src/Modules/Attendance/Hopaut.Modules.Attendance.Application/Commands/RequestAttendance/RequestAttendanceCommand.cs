using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed record RequestAttendanceCommand(PostId PostId, UserId UserId) : ICommand<Result>, IAttendanceCommand;
