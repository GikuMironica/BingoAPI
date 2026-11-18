using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain.Events;

/// <summary>A user asked to attend a post; <paramref name="AwaitsApproval"/> is false when the request was accepted at once (events other than house parties).</summary>
public sealed record AttendanceRequestedDomainEvent(
    DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId, bool AwaitsApproval) : DomainEventBase(OccurredOnUtc);

/// <summary>The host accepted a pending request.</summary>
public sealed record AttendanceAcceptedDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

/// <summary>The host turned down a pending request or removed an accepted attendee.</summary>
public sealed record AttendanceRejectedDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

/// <summary>The participant withdrew a pending request or an accepted attendance.</summary>
public sealed record AttendanceCancelledDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);
