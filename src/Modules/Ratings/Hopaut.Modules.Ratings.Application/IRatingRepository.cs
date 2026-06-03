using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application;

public interface IRatingRepository
{
    Task<Rating?> GetByIdAsync(RatingId id, CancellationToken ct = default);
    Task<List<Rating>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    Task<List<Rating>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    Task AddAsync(Rating rating, CancellationToken ct = default);
    Task DeleteAsync(Rating rating, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
