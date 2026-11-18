using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application.Queries;

/// <summary>Attendance reads served by no-tracking queries, filtered and paged in SQL (never loading aggregates).</summary>
public interface IAttendanceReadModel
{
    /// <summary>Whether the user's participation in the post is accepted.</summary>
    Task<bool> IsAcceptedAsync(PostId postId, UserId userId, CancellationToken ct);

    /// <summary>Up to <paramref name="count"/> participations of the post in the given status, earliest request first, after the given position.</summary>
    Task<IReadOnlyList<Attendee>> GetAttendeesAsync(
        PostId postId, AttendanceStatus status, AttendeeKeyset? after, int count, CancellationToken ct);
}
