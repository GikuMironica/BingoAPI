using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain.Events;

public sealed record PostCreatedDomainEvent(DateTimeOffset OccurredOnUtc, PostId PostId, UserId UserId) : DomainEventBase(OccurredOnUtc);

public sealed record PostDeletedDomainEvent(DateTimeOffset OccurredOnUtc, PostId PostId) : DomainEventBase(OccurredOnUtc);

/// <summary>The host changed the post; <paramref name="NotifyAttendees"/> when its time, place, requirements or fee changed.</summary>
public sealed record PostUpdatedDomainEvent(DateTimeOffset OccurredOnUtc, PostId PostId, bool NotifyAttendees) : DomainEventBase(OccurredOnUtc);

public sealed record PictureStateChangedDomainEvent(DateTimeOffset OccurredOnUtc, PictureId PictureId, PostId PostId, PictureState NewState) : DomainEventBase(OccurredOnUtc);
