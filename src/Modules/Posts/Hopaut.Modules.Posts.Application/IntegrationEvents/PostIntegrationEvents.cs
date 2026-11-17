using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application.IntegrationEvents;

public sealed record PostCreatedIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId, UserId HostUserId) : IntegrationEventBase(OccurredOnUtc);

public sealed record PostDeletedIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

public sealed record PictureStateChangedIntegrationEvent(DateTimeOffset OccurredOnUtc, PictureId PictureId, PostId PostId, string NewState) : IntegrationEventBase(OccurredOnUtc);
