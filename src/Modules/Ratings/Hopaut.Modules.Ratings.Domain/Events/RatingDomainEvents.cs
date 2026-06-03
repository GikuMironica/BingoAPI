using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain.Events;

public sealed record RatingCreatedDomainEvent(RatingId RatingId, UserId UserId, UserId RaterId, PostId PostId) : DomainEventBase;

public sealed record RatingDeletedDomainEvent(RatingId RatingId, UserId UserId) : DomainEventBase;
