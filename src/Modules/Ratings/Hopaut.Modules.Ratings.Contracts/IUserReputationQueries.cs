using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Contracts;

/// <summary>Read-only reputation lookup offered to other modules; batch-shaped, so callers never query per item.</summary>
public interface IUserReputationQueries
{
    /// <summary>Reputations of the given users; users without ratings are absent from the result.</summary>
    Task<IReadOnlyDictionary<UserId, UserReputationSummary>> GetAsync(IReadOnlyCollection<UserId> userIds, CancellationToken cancellationToken);
}

public sealed record UserReputationSummary(UserId UserId, int TotalRatings, double AverageRating);
