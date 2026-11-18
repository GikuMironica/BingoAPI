namespace Hopaut.Modules.Attendance.Application.Authorization;

/// <summary>Resource-based policies of the Attendance module (plan Step 6.9), registered by <c>AddAttendanceModule</c>.</summary>
public static class AttendancePolicies
{
    /// <summary>
    /// The caller hosts the post; administrators do not decide attendance (the legacy rule for accepting and rejecting
    /// attendees). The resource is the post's <c>PostHost</c> from Posts.Contracts.
    /// </summary>
    public const string MustHostPost = "attendance.post.host";
}
