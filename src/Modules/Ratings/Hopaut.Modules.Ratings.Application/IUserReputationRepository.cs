using Hopaut.Modules.Ratings.Domain;

namespace Hopaut.Modules.Ratings.Application;

public interface IUserReputationRepository
{
    Task<UserReputation?> GetAsync(string userId, CancellationToken ct = default);
    Task<Dictionary<string, UserReputation>> GetBatchAsync(IEnumerable<string> userIds, CancellationToken ct = default);
    Task UpsertAsync(string userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
