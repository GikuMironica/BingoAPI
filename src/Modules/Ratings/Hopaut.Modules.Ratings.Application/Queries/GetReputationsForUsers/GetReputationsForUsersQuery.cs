using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;

public sealed record GetReputationsForUsersQuery(IReadOnlyList<UserId> UserIds) : IRequest<Dictionary<UserId, UserReputationDto>>;

public sealed record UserReputationDto(UserId UserId, int TotalRatings, double AverageRating);
