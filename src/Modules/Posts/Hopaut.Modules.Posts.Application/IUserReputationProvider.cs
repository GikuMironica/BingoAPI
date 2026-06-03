using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application;

/// <summary>
/// Cross-module contract for batch-fetching user reputations.
/// Implemented by an adapter in the host/infrastructure that calls the Ratings module.
/// </summary>
public interface IUserReputationProvider
{
    Task<Dictionary<UserId, UserReputationInfo>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default);
}

public sealed record UserReputationInfo(int TotalRatings, double AverageRating);
