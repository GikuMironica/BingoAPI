using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;

public sealed class GetRatingsByUserQueryHandler(IRatingReadModel readModel)
    : IQueryHandler<GetRatingsByUserQuery, Result<IReadOnlyList<RatingDto>>>
{
    public async ValueTask<Result<IReadOnlyList<RatingDto>>> Handle(GetRatingsByUserQuery request, CancellationToken cancellationToken) =>
        Result.Success(await readModel.GetReceivedAsync(request.UserId, cancellationToken));
}
