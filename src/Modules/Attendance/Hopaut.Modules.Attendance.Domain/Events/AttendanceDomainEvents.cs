using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain.Events;

public sealed record AttendanceRequestedDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

public sealed record AttendanceAcceptedDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

public sealed record AttendanceRejectedDomainEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);
