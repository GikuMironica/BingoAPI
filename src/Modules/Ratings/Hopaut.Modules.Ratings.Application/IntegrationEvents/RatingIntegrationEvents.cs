using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application.IntegrationEvents;

public sealed record RatingCreatedIntegrationEvent(RatingId RatingId, UserId UserId, UserId RaterId, PostId PostId) : IntegrationEventBase;

public sealed record RatingDeletedIntegrationEvent(RatingId RatingId, UserId UserId) : IntegrationEventBase;
