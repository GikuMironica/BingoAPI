using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Contracts;

/// <summary>Read-only attendance facts offered to other modules (the Posts capacity rule, plan Step 8.2).</summary>
public interface IAttendanceQueries
{
    /// <summary>How many attendees of the post have been accepted.</summary>
    Task<int> CountAcceptedAsync(PostId postId, CancellationToken cancellationToken);
}
