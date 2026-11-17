using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

/// <summary>The host of the post accepts an attendee's request (<see cref="Authorization.AttendancePolicies.MustHostPost"/>).</summary>
public sealed record AcceptAttendanceCommand(PostId PostId, UserId UserId) : ICommand<Result>, IAttendanceCommand;
