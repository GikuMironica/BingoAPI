using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain.Events;

public sealed record PostCreatedDomainEvent(PostId PostId, UserId UserId) : DomainEventBase;

public sealed record PostDeletedDomainEvent(PostId PostId) : DomainEventBase;

public sealed record PictureStateChangedDomainEvent(PictureId PictureId, PostId PostId, PictureState NewState) : DomainEventBase;
