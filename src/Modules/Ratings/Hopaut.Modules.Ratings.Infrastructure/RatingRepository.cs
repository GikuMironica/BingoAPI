using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public sealed class RatingRepository(RatingsModuleDbContext db) : IRatingRepository
{
    public async Task<RatingId> NextIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(RatingsModuleDbContext.SchemaName, RatingsModuleDbContext.RatingIdSequence, ct));

    public Task<Rating?> GetByIdAsync(RatingId id, CancellationToken ct = default)
        => db.Ratings.FirstOrDefaultAsync(r => r.Id == id, ct);

    // The unique (RaterId, PostId) index serves the check and rejects a concurrent duplicate on save.
    public Task<bool> ExistsAsync(UserId raterId, PostId postId, CancellationToken ct = default)
        => db.Ratings.AnyAsync(r => r.RaterId == raterId && r.PostId == postId, ct);

    public void Add(Rating rating) => db.Ratings.Add(rating);

    public void Remove(Rating rating) => db.Ratings.Remove(rating);
}
