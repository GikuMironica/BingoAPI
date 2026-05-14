using Hopaut.Modules.Ratings.Domain;

namespace Hopaut.Modules.Ratings.Application;

public interface IRatingRepository
{
    Task<Rating?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Rating>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<List<Rating>> GetByPostAsync(int postId, CancellationToken ct = default);
    Task AddAsync(Rating rating, CancellationToken ct = default);
    Task DeleteAsync(Rating rating, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
