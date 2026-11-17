using Hopaut.Modules.Ratings.Contracts;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

/// <summary>Serves <see cref="IUserReputationQueries"/> from the reputation projection with one no-tracking query.</summary>
public sealed class UserReputationQueries(RatingsModuleDbContext db) : IUserReputationQueries
{
    public async Task<IReadOnlyDictionary<UserId, UserReputationSummary>> GetAsync(
        IReadOnlyCollection<UserId> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<UserId, UserReputationSummary>();
        }

        var ids = userIds.Distinct().ToList();
        return await db.UserReputations
            .AsNoTracking()
            .Where(r => ids.Contains(r.UserId))
            .ToDictionaryAsync(
                r => r.UserId,
                r => new UserReputationSummary(r.UserId, r.TotalRatings, r.AverageRating),
                cancellationToken);
    }
}
