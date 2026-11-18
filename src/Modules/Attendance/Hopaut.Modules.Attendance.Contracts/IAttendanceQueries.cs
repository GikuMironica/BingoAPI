using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Contracts;

/// <summary>Read-only attendance facts offered to other modules (the Posts capacity rule and the caller's attended posts).</summary>
public interface IAttendanceQueries
{
    /// <summary>How many attendees of the post have been accepted.</summary>
    Task<int> CountAcceptedAsync(PostId postId, CancellationToken cancellationToken);

    /// <summary>The posts in which the user's participation is accepted.</summary>
    Task<IReadOnlyList<PostId>> GetAcceptedPostIdsAsync(UserId userId, CancellationToken cancellationToken);

    /// <summary>Whether the user's participation in the post is accepted (the Ratings eligibility rule).</summary>
    Task<bool> IsAcceptedAsync(PostId postId, UserId userId, CancellationToken cancellationToken);
}
