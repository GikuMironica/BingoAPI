using Hopaut.Modules.Ratings.Application.Queries;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

/// <summary>Serves <see cref="IRatingReadModel"/> with no-tracking projections.</summary>
public sealed class RatingReadModel(RatingsModuleDbContext db) : IRatingReadModel
{
    public Task<RatingDto?> GetByIdAsync(RatingId ratingId, CancellationToken ct) =>
        db.Ratings
            .AsNoTracking()
            .Where(r => r.Id == ratingId)
            .Select(r => new RatingDto(r.Id.Value, r.Rate.Value, r.RaterId.Value, r.PostId.Value, r.Feedback == null ? null : r.Feedback.Value, r.CreatedAt))
            .SingleOrDefaultAsync(ct);

    // The UserId index serves the filter.
    public async Task<IReadOnlyList<RatingDto>> GetReceivedAsync(UserId userId, CancellationToken ct) =>
        await db.Ratings
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingDto(r.Id.Value, r.Rate.Value, r.RaterId.Value, r.PostId.Value, r.Feedback == null ? null : r.Feedback.Value, r.CreatedAt))
            .ToListAsync(ct);
}
