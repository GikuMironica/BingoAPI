using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;

public sealed class GetReputationsForUsersQueryHandler(IUserReputationRepository reputations)
    : IQueryHandler<GetReputationsForUsersQuery, Result<IReadOnlyDictionary<UserId, UserReputationDto>>>
{
    public async ValueTask<Result<IReadOnlyDictionary<UserId, UserReputationDto>>> Handle(
        GetReputationsForUsersQuery request, CancellationToken cancellationToken)
    {
        var stored = await reputations.GetBatchAsync(request.UserIds, cancellationToken);
        IReadOnlyDictionary<UserId, UserReputationDto> dtos = stored.ToDictionary(
            kvp => kvp.Key,
            kvp => new UserReputationDto(kvp.Key, kvp.Value.TotalRatings, kvp.Value.AverageRating));
        return Result.Success(dtos);
    }
}
