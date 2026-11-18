using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Contracts;

public sealed record RatingCreatedIntegrationEvent(DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId, UserId RaterId, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

public sealed record RatingDeletedIntegrationEvent(DateTimeOffset OccurredOnUtc, RatingId RatingId, UserId UserId) : IntegrationEventBase(OccurredOnUtc);
