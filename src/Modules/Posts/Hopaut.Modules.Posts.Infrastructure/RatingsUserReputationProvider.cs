using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Ratings.Application.Queries.GetReputationsForUsers;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>
/// Reads reputations from the Ratings module by sending its query. Transitional: plan Step 6.6 replaces this
/// cross-module call (finding F-023) with a read-only query interface in the Ratings Contracts project.
/// </summary>
public sealed class RatingsUserReputationProvider(ISender sender) : IUserReputationProvider
{
    public async Task<Dictionary<UserId, UserReputationInfo>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        if (ids.Count == 0) return new();

        var result = await sender.Send(new GetReputationsForUsersQuery(ids), ct);

        // The query has no failure path; Value throws if that ever changes.
        return result.Value.ToDictionary(
            kvp => kvp.Key,
            kvp => new UserReputationInfo(kvp.Value.TotalRatings, kvp.Value.AverageRating));
    }
}
