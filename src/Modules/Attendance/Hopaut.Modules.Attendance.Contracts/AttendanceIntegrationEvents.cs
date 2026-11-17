using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Contracts;

/// <summary>A user asked to attend a post; <paramref name="AwaitsApproval"/> is false when the request was accepted at once (events other than house parties).</summary>
public sealed record AttendanceRequestedIntegrationEvent(
    DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId, bool AwaitsApproval) : IntegrationEventBase(OccurredOnUtc);

/// <summary>The host accepted a pending request.</summary>
public sealed record AttendanceAcceptedIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);

/// <summary>The host turned down a pending request or removed an accepted attendee.</summary>
public sealed record AttendanceRejectedIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);

/// <summary>The participant withdrew a pending request or an accepted attendance.</summary>
public sealed record AttendanceCancelledIntegrationEvent(DateTimeOffset OccurredOnUtc, ParticipationId ParticipationId, PostId PostId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);
