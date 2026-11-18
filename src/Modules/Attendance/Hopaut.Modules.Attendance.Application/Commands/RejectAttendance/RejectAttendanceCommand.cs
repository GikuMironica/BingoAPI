using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

/// <summary>The host of the post rejects an attendee's request (<see cref="Authorization.AttendancePolicies.MustHostPost"/>).</summary>
public sealed record RejectAttendanceCommand(PostId PostId, UserId UserId) : ICommand<Result>, IAttendanceCommand;
