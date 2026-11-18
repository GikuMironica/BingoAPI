using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;

public sealed record GetRatingsByUserQuery(UserId UserId) : IQuery<Result<IReadOnlyList<RatingDto>>>;
