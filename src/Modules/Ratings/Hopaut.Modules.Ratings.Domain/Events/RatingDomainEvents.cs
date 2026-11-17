using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain.Events;

public sealed record RatingCreatedDomainEvent(DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId, UserId RaterId, PostId PostId) : DomainEventBase(OccurredOnUtc);

public sealed record RatingDeletedDomainEvent(DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId) : DomainEventBase(OccurredOnUtc);
