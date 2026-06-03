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

    public static Participation Create(PostId postId, UserId userId)
    {
        var participation = new Participation
        {
            PostId = postId,
            UserId = userId,
            Status = AttendanceStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        };

        participation.AddDomainEvent(new AttendanceRequestedDomainEvent(participation.Id, postId, userId));

        return participation;
    }

    public bool Accept()
    {
        if (Status != AttendanceStatus.Pending) return false;
        Status = AttendanceStatus.Accepted;
        RespondedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AttendanceAcceptedDomainEvent(Id, PostId, UserId));
        return true;
    }

    public bool Reject()
    {
        if (Status != AttendanceStatus.Pending) return false;
        Status = AttendanceStatus.Rejected;
        RespondedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new AttendanceRejectedDomainEvent(Id, PostId, UserId));
        return true;
    }

    public bool Cancel()
    {
        if (Status == AttendanceStatus.Cancelled) return false;
        Status = AttendanceStatus.Cancelled;
        RespondedAt = DateTimeOffset.UtcNow;
        return true;
    }
}
