using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Contracts;

public sealed record AttendanceRequestedIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);

public sealed record AttendanceAcceptedIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);

public sealed record AttendanceRejectedIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);
