using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed record CancelAttendanceCommand(PostId PostId, UserId UserId) : ICommand<Result>, IAttendanceCommand;
