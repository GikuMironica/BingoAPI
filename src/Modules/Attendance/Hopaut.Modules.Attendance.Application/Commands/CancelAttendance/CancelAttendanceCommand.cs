using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

/// <summary>The caller withdraws their pending request or accepted attendance.</summary>
public sealed record CancelAttendanceCommand(PostId PostId) : ICommand<Result>, IAttendanceCommand;
