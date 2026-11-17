using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

public sealed record RejectAttendanceCommand(PostId PostId, UserId UserId, UserId OwnerId) : ICommand<Result>, IAttendanceCommand;
