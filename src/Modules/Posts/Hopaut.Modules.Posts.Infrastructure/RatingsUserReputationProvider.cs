using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Ratings.Contracts;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Reads reputations through the Ratings module's public query interface (its Contracts project).</summary>
public sealed class RatingsUserReputationProvider(IUserReputationQueries reputations) : IUserReputationProvider
{
    public async Task<Dictionary<UserId, UserReputationInfo>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0) return new();

        var found = await reputations.GetAsync(ids, ct);
        return found.ToDictionary(
            kvp => kvp.Key,
            kvp => new UserReputationInfo(kvp.Value.TotalRatings, kvp.Value.AverageRating));
    }
}
