using MediatR;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;

public sealed class GetRatingsByUserQueryHandler : IRequestHandler<GetRatingsByUserQuery, IReadOnlyList<RatingDto>>
{
    private readonly IRatingRepository _repo;

    public GetRatingsByUserQueryHandler(IRatingRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<RatingDto>> Handle(GetRatingsByUserQuery request, CancellationToken cancellationToken)
    {
        var ratings = await _repo.GetByUserAsync(request.UserId, cancellationToken);
        return ratings.Select(r => new RatingDto(r.Id, r.Rate, r.RaterId, r.PostId, r.Feedback, r.CreatedAt)).ToList();
    }
}
