using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain;

/// <summary>Expected failures of the Participation aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class ParticipationErrors
{
    public static Error NotFound(PostId postId, UserId userId) =>
        Error.NotFound("attendance.participation.not_found", $"User {userId} has not requested to attend post {postId}.");

    public static Error AlreadyRequested(PostId postId, UserId userId) =>
        Error.Conflict("attendance.participation.already_requested", $"User {userId} already requested to attend post {postId}.");

    public static Error NotPending(ParticipationId participationId) =>
        Error.Conflict("attendance.participation.not_pending", $"Participation {participationId} has already been decided.");

    public static Error AlreadyCancelled(ParticipationId participationId) =>
        Error.Conflict("attendance.participation.already_cancelled", $"Participation {participationId} is already cancelled.");

    public static Error PostNotFound(PostId postId) =>
        Error.NotFound("attendance.participation.post_not_found", $"Post {postId} was not found.");

    public static Error NotPostHost(PostId postId) =>
        Error.Forbidden("attendance.participation.not_post_host", $"Only the host of post {postId} can decide its attendance requests.");
}
