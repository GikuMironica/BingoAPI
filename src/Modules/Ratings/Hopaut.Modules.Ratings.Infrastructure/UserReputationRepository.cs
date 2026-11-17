using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public sealed class UserReputationRepository(RatingsModuleDbContext db) : IUserReputationRepository
{
    public Task<UserReputation?> GetAsync(UserId userId, CancellationToken ct = default)
        => db.UserReputations.FirstOrDefaultAsync(r => r.UserId == userId, ct);

    public async Task<Dictionary<UserId, UserReputation>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        var reputations = await db.UserReputations
            .Where(r => ids.Contains(r.UserId))
            .ToListAsync(ct);
        return reputations.ToDictionary(r => r.UserId);
    }

    // TODO (F-017, plan Steps 8.4/9.3): concurrent rating changes can lose updates; add a concurrency token.
    public async Task RecalculateAsync(UserId userId, CancellationToken ct = default)
    {
        var stored = await db.Ratings
            .Where(r => r.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Sum = g.Sum(r => r.Rate) })
            .FirstOrDefaultAsync(ct);

        // Ratings added or removed by this unit of work are not in the database yet (one save per command).
        var pending = db.ChangeTracker.Entries<Rating>()
            .Where(entry => entry.Entity.UserId == userId && entry.State is EntityState.Added or EntityState.Deleted)
            .Select(entry => (Sign: entry.State == EntityState.Added ? 1 : -1, Rate: entry.Entity.Rate.Value))
            .ToList();

        var count = (stored?.Count ?? 0) + pending.Sum(p => p.Sign);
        var sum = (stored?.Sum ?? 0) + pending.Sum(p => p.Sign * p.Rate);

        var existing = await db.UserReputations.FirstOrDefaultAsync(r => r.UserId == userId, ct);

        if (count <= 0)
        {
            if (existing is not null) db.UserReputations.Remove(existing);
            return;
        }

        if (existing is null)
        {
            existing = new UserReputation { UserId = userId };
            db.UserReputations.Add(existing);
        }

        existing.TotalRatings = count;
        existing.SumRatings = sum;
        existing.AverageRating = (double)sum / count;
    }
}
