using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application;

public interface IUserReputationRepository
{
    Task<UserReputation?> GetAsync(UserId userId, CancellationToken ct = default);
    Task<Dictionary<UserId, UserReputation>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default);
    Task UpsertAsync(UserId userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
