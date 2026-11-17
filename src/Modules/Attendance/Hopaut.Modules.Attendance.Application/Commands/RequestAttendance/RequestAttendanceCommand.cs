using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

/// <summary>The caller asks to attend the post, or asks again after a rejection or a cancellation (decision D-030).</summary>
public sealed record RequestAttendanceCommand(PostId PostId) : ICommand<Result>, IAttendanceCommand;
