using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Domain.Events;

public sealed record AttendanceRequestedDomainEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase;

public sealed record AttendanceAcceptedDomainEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase;

public sealed record AttendanceRejectedDomainEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : DomainEventBase;
