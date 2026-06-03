using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Queries.GetRatingsByUser;

public sealed record GetRatingsByUserQuery(UserId UserId) : IRequest<IReadOnlyList<RatingDto>>;

public sealed record RatingDto(int Id, int Rate, string RaterId, int PostId, string? Feedback, DateTimeOffset CreatedAt);
