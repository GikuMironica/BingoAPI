using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;

public sealed record GetReputationsForUsersQuery(IReadOnlyList<UserId> UserIds)
    : IQuery<Result<IReadOnlyDictionary<UserId, UserReputationDto>>>;

public sealed record UserReputationDto(UserId UserId, int TotalRatings, double AverageRating);
