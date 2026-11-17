using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingById;

public sealed class GetRatingByIdQueryHandler(IRatingReadModel readModel) : IQueryHandler<GetRatingByIdQuery, Result<RatingDto>>
{
    public async ValueTask<Result<RatingDto>> Handle(GetRatingByIdQuery request, CancellationToken cancellationToken) =>
        await readModel.GetByIdAsync(request.RatingId, cancellationToken) is { } rating
            ? rating
            : RatingErrors.NotFound(request.RatingId);
}
