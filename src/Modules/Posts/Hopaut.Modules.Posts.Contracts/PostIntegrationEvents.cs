using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Contracts;

public sealed record PostCreatedIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId, UserId HostUserId) : IntegrationEventBase(OccurredOnUtc);

public sealed record PostDeletedIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

/// <summary>The host changed a post; attendees are notified (plan Step 9.4) when <paramref name="NotifyAttendees"/> is set.</summary>
public sealed record PostUpdatedIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId, bool NotifyAttendees) : IntegrationEventBase(OccurredOnUtc);

public sealed record PictureStateChangedIntegrationEvent(DateTimeOffset OccurredOnUtc, PictureId PictureId, PostId PostId, string NewState) : IntegrationEventBase(OccurredOnUtc);
