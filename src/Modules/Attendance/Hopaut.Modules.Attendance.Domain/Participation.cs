using Hopaut.Modules.Attendance.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain;

/// <summary>
/// A user's attendance of a post (rules recovered from the legacy API, decision D-030). A house-party request waits for the
/// host; other events accept a request at once. Pending and accepted participations are active: the host accepts a pending
/// one while slots are left and rejects an active one, the participant cancels an active one, and a rejected or cancelled
/// participant may request again.
/// </summary>
public sealed class Participation : AggregateRoot<ParticipationId>
{
    private Participation() { } // EF Core

    public PostId PostId { get; private set; }

    public UserId UserId { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    /// <summary>Optimistic-concurrency token (PostgreSQL <c>xmin</c>): the host and the participant change a participation independently.</summary>
    public uint Version { get; private set; }

    /// <summary>Pending or accepted: the participant still wants to attend and has not been turned away.</summary>
    public bool IsActive => Status is AttendanceStatus.Pending or AttendanceStatus.Accepted;

    /// <summary>
    /// Requests to attend the post with an ID taken from the participations sequence, so the requested event carries it.
    /// <paramref name="acceptedAttendees"/> is the number of accepted participations of the post.
    /// </summary>
    public static Result<Participation> Request(
        ParticipationId id, AttendanceTerms post, UserId requester, int acceptedAttendees, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(post);

        if (Refusal(post, requester, acceptedAttendees, now) is { } refusal)
            return refusal;

        var participation = new Participation { Id = id, PostId = post.PostId, UserId = requester };
        participation.Open(post, now);
        return participation;
    }

    /// <summary>A rejected or cancelled participant requests again, under the same rules as a first request.</summary>
    public Result RequestAgain(AttendanceTerms post, int acceptedAttendees, DateTimeOffset now)
    {
        EnsureTermsOfThisPost(post);
        if (IsActive)
            return ParticipationErrors.AlreadyRequested(PostId, UserId);

        if (Refusal(post, UserId, acceptedAttendees, now) is { } refusal)
            return refusal;

        Open(post, now);
        return Result.Success();
    }

    /// <summary>The host accepts a pending request while the post has slots left.</summary>
    public Result Accept(AttendanceTerms post, int acceptedAttendees, DateTimeOffset now)
    {
        EnsureTermsOfThisPost(post);
        if (Status != AttendanceStatus.Pending)
            return ParticipationErrors.NotPending(Id);

        if (post.IsFull(acceptedAttendees))
            return ParticipationErrors.NoSlotsLeft(PostId);

        Status = AttendanceStatus.Accepted;
        RespondedAt = now;
        Raise(new AttendanceAcceptedDomainEvent(now, Id, PostId, UserId));
        return Result.Success();
    }

    /// <summary>The host turns down a pending request or removes an accepted attendee.</summary>
    public Result Reject(DateTimeOffset now)
    {
        if (!IsActive)
            return ParticipationErrors.NotActive(Id);

        Status = AttendanceStatus.Rejected;
        RespondedAt = now;
        Raise(new AttendanceRejectedDomainEvent(now, Id, PostId, UserId));
        return Result.Success();
    }

    /// <summary>The participant withdraws a pending request or an accepted attendance.</summary>
    public Result Cancel(DateTimeOffset now)
    {
        if (Status == AttendanceStatus.Cancelled)
            return ParticipationErrors.AlreadyCancelled(Id);

        if (!IsActive)
            return ParticipationErrors.NotActive(Id);

        Status = AttendanceStatus.Cancelled;
        RespondedAt = now;
        Raise(new AttendanceCancelledDomainEvent(now, Id, PostId, UserId));
        return Result.Success();
    }

    private static Error? Refusal(AttendanceTerms post, UserId requester, int acceptedAttendees, DateTimeOffset now)
    {
        if (requester == post.HostId)
            return ParticipationErrors.OwnPost(post.PostId);

        if (!post.IsListed)
            return ParticipationErrors.PostDisabled(post.PostId);

        if (post.HasEndedAt(now))
            return ParticipationErrors.PostEnded(post.PostId);

        return post.IsFull(acceptedAttendees) ? ParticipationErrors.NoSlotsLeft(post.PostId) : null;
    }

    private void Open(AttendanceTerms post, DateTimeOffset now)
    {
        var awaitsApproval = post.HostApprovesAttendees;
        Status = awaitsApproval ? AttendanceStatus.Pending : AttendanceStatus.Accepted;
        RequestedAt = now;
        RespondedAt = awaitsApproval ? null : now;
        Raise(new AttendanceRequestedDomainEvent(now, Id, PostId, UserId, awaitsApproval));
    }

    private void EnsureTermsOfThisPost(AttendanceTerms post)
    {
        ArgumentNullException.ThrowIfNull(post);
        if (post.PostId != PostId)
            throw new ArgumentException($"The terms of post {post.PostId} do not apply to participation {Id} of post {PostId}.", nameof(post));
    }
}
