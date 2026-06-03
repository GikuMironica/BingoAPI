using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application.IntegrationEvents;

public sealed record AttendanceRequestedIntegrationEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase;

public sealed record AttendanceAcceptedIntegrationEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase;

public sealed record AttendanceRejectedIntegrationEvent(ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase;
