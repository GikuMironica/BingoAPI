using Hopaut.Modules.Ratings.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Ratings.Application;

/// <summary>Rating aggregate store. Changes are saved by the Ratings unit of work, never by the repository.</summary>
public interface IRatingRepository
{
    /// <summary>Next ID from the ratings sequence, assigned before the rating is created.</summary>
    Task<RatingId> NextIdAsync(CancellationToken ct = default);
    Task<Rating?> GetByIdAsync(RatingId id, CancellationToken ct = default);
    Task<List<Rating>> GetByUserAsync(UserId userId, CancellationToken ct = default);
    Task<List<Rating>> GetByPostAsync(PostId postId, CancellationToken ct = default);
    void Add(Rating rating);
    void Remove(Rating rating);
}
