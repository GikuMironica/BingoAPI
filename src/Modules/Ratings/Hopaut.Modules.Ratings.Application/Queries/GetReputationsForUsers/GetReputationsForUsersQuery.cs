using MediatR;

namespace Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;

public sealed record GetReputationsForUsersQuery(IReadOnlyList<string> UserIds) : IRequest<Dictionary<string, UserReputationDto>>;

public sealed record UserReputationDto(string UserId, int TotalRatings, double AverageRating);
