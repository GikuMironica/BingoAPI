using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public sealed class UserReputationRepository : IUserReputationRepository
{
    private readonly RatingsModuleDbContext _db;

    public UserReputationRepository(RatingsModuleDbContext db) => _db = db;

    public Task<UserReputation?> GetAsync(UserId userId, CancellationToken ct = default)
        => _db.UserReputations.FirstOrDefaultAsync(r => r.UserId == userId, ct);

    public async Task<Dictionary<UserId, UserReputation>> GetBatchAsync(IEnumerable<UserId> userIds, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        var reputations = await _db.UserReputations
            .Where(r => ids.Contains(r.UserId))
            .ToListAsync(ct);
        return reputations.ToDictionary(r => r.UserId);
    }

    public async Task UpsertAsync(UserId userId, CancellationToken ct = default)
    {
        var stats = await _db.Ratings
            .Where(r => r.UserId == userId)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Sum = g.Sum(r => r.Rate) })
            .FirstOrDefaultAsync(ct);

        var existing = await _db.UserReputations.FirstOrDefaultAsync(r => r.UserId == userId, ct);

        if (stats is null || stats.Count == 0)
        {
            if (existing is not null) _db.UserReputations.Remove(existing);
            return;
        }

        if (existing is null)
        {
            existing = new UserReputation { UserId = userId };
            await _db.UserReputations.AddAsync(existing, ct);
        }

        existing.TotalRatings = stats.Count;
        existing.SumRatings = stats.Sum;
        existing.AverageRating = (double)stats.Sum / stats.Count;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
