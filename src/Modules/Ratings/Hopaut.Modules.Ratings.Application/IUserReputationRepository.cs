using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application;

public interface IUserReputationRepository
{
    Task<UserReputation?> GetAsync(UserId userId, CancellationToken ct = default);
    Task<Dictionary<UserId, UserReputation>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default);

    /// <summary>
    /// Recomputes the user's reputation from the stored ratings and the ratings the current unit of work adds or removes,
    /// which are not saved yet. The Ratings unit of work saves the projection together with the rating change.
    /// </summary>
    Task RecalculateAsync(UserId userId, CancellationToken ct = default);
}
