using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed record AcceptAttendanceCommand(PostId PostId, UserId UserId, UserId OwnerId) : ICommand<Result>, IAttendanceCommand;
