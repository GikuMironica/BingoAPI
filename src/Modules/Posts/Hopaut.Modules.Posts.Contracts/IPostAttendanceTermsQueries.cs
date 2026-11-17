using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Contracts;

/// <summary>Read-only lookup of the terms under which users may attend a post, offered to the Attendance rules (plan Step 8.3).</summary>
public interface IPostAttendanceTermsQueries
{
    /// <summary>The post's attendance terms, or null when the post does not exist.</summary>
    Task<PostAttendanceTerms?> GetAsync(PostId postId, CancellationToken cancellationToken);
}

/// <summary>
/// What attending a post depends on: its host, whether it is listed (not disabled), when it ends (null: no end), whether the
/// host approves each attendee, and how many attendees it admits (null: no limit).
/// </summary>
public sealed record PostAttendanceTerms(PostHost Host, bool IsListed, DateTimeOffset? EndsAt, bool HostApprovesAttendees, int? Slots);
