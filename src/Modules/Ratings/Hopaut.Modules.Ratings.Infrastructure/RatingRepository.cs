using Hopaut.Modules.Ratings.Application;
using Hopaut.Modules.Ratings.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Ratings.Infrastructure;

public sealed class RatingRepository : IRatingRepository
{
    private readonly RatingsModuleDbContext _db;

    public RatingRepository(RatingsModuleDbContext db) => _db = db;

    public Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Ratings.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<Rating>> GetByUserAsync(string userId, CancellationToken ct = default)
        => _db.Ratings.Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public Task<List<Rating>> GetByPostAsync(int postId, CancellationToken ct = default)
        => _db.Ratings.Where(r => r.PostId == postId).ToListAsync(ct);

    public async Task AddAsync(Rating rating, CancellationToken ct = default)
        => await _db.Ratings.AddAsync(rating, ct);

    public Task DeleteAsync(Rating rating, CancellationToken ct = default)
    {
        _db.Ratings.Remove(rating);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
