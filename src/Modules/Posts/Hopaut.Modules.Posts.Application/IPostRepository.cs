using Hopaut.Modules.Posts.Domain;

namespace Hopaut.Modules.Posts.Application;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Post>> GetNearbyAsync(double longitude, double latitude, double radiusMeters, int limit, CancellationToken ct = default);
    Task AddAsync(Post post, CancellationToken ct = default);
    Task DeleteAsync(Post post, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
