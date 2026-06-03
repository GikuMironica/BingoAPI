using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;

public sealed class GetReputationsForUsersQueryHandler : IRequestHandler<GetReputationsForUsersQuery, Dictionary<UserId, UserReputationDto>>
{
    private readonly IUserReputationRepository _repo;

    public GetReputationsForUsersQueryHandler(IUserReputationRepository repo) => _repo = repo;

    public async Task<Dictionary<UserId, UserReputationDto>> Handle(GetReputationsForUsersQuery request, CancellationToken cancellationToken)
    {
        var reputations = await _repo.GetBatchAsync(request.UserIds, cancellationToken);
        return reputations.ToDictionary(
            kvp => kvp.Key,
            kvp => new UserReputationDto(kvp.Key, kvp.Value.TotalRatings, kvp.Value.AverageRating));
    }
}
