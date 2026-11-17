using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain.Events;

public sealed record PostCreatedDomainEvent(DateTimeOffset OccurredOnUtc, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

public sealed record PostDeletedDomainEvent(DateTimeOffset OccurredOnUtc, PostId PostId) : DomainEventBase(OccurredOnUtc);

public sealed record PictureStateChangedDomainEvent(DateTimeOffset OccurredOnUtc, PictureId PictureId, PostId PostId, PictureState NewState) : DomainEventBase(OccurredOnUtc);
