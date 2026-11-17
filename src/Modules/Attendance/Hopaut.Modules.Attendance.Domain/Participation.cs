using Hopaut.Modules.Attendance.Domain.Events;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain;

public sealed class Participation : AggregateRoot<ParticipationId>
{
    public PostId PostId { get; private set; }
    public UserId UserId { get; private set; }
    public AttendanceStatus Status { get; private set; } = AttendanceStatus.Pending;
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    private Participation() { } // EF Core

    /// <summary>Creates a request with an ID taken from the participations sequence, so the requested event carries it.</summary>
    public static Participation Create(ParticipationId id, PostId postId, UserId userId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);

        var participation = new Participation
        {
            Id = id,
            PostId = postId,
            UserId = userId,
            Status = AttendanceStatus.Pending,
            RequestedAt = now
        };

        participation.Raise(new AttendanceRequestedDomainEvent(now, id, postId, userId));

        return participation;
    }

    public bool Accept(DateTimeOffset now)
    {
        if (Status != AttendanceStatus.Pending) return false;
        Status = AttendanceStatus.Accepted;
        RespondedAt = now;
        Raise(new AttendanceAcceptedDomainEvent(now, Id, PostId, UserId));
        return true;
    }

    public bool Reject(DateTimeOffset now)
    {
        if (Status != AttendanceStatus.Pending) return false;
        Status = AttendanceStatus.Rejected;
        RespondedAt = now;
        Raise(new AttendanceRejectedDomainEvent(now, Id, PostId, UserId));
        return true;
    }

    public bool Cancel(DateTimeOffset now)
    {
        if (Status == AttendanceStatus.Cancelled) return false;
        Status = AttendanceStatus.Cancelled;
        RespondedAt = now;
        return true;
    }
}
