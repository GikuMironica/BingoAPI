using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application.IntegrationEvents;

public sealed record PostCreatedIntegrationEvent(PostId PostId, UserId HostUserId) : IntegrationEventBase;

public sealed record PostDeletedIntegrationEvent(PostId PostId) : IntegrationEventBase;

public sealed record PictureStateChangedIntegrationEvent(PictureId PictureId, PostId PostId, string NewState) : IntegrationEventBase;
