using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Domain.Events;

/// <summary>An attendee rated the host (<paramref name="UserId"/>) of a post.</summary>
public sealed record RatingCreatedDomainEvent(
    DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId, UserId RaterId, PostId PostId, RatingValue Rate) : DomainEventBase(OccurredOnUtc);

/// <summary>A rating of the user <paramref name="UserId"/> was deleted.</summary>
public sealed record RatingDeletedDomainEvent(DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId, RatingValue Rate) : DomainEventBase(OccurredOnUtc);
