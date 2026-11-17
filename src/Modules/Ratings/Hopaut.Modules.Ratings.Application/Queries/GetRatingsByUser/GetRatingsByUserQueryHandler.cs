using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;

public sealed class GetRatingsByUserQueryHandler(IRatingRepository ratings)
    : IQueryHandler<GetRatingsByUserQuery, Result<IReadOnlyList<RatingDto>>>
{
    public async ValueTask<Result<IReadOnlyList<RatingDto>>> Handle(GetRatingsByUserQuery request, CancellationToken cancellationToken)
    {
        var received = await ratings.GetByUserAsync(request.UserId, cancellationToken);
        IReadOnlyList<RatingDto> dtos = received
            .Select(r => new RatingDto(r.Id.Value, r.Rate.Value, r.RaterId.Value, r.PostId.Value, r.Feedback, r.CreatedAt))
            .ToList();
        return Result.Success(dtos);
    }
}
