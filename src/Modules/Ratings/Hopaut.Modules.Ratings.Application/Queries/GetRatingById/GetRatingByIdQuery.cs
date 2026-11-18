using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingById;

public sealed record GetRatingByIdQuery(RatingId RatingId) : IQuery<Result<RatingDto>>;
