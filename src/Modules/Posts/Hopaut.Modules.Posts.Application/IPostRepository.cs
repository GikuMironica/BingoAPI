using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application;

/// <summary>Post aggregate store. Changes are saved by the Posts unit of work, never by the repository.</summary>
public interface IPostRepository
{
    /// <summary>Next ID from the posts sequence, assigned before the post is created.</summary>
    Task<PostId> NextIdAsync(CancellationToken ct = default);
    Task<Post?> GetByIdAsync(PostId id, CancellationToken ct = default);
    Task<IReadOnlyList<Post>> GetNearbyAsync(double longitude, double latitude, double radiusMeters, int limit, CancellationToken ct = default);
    void Add(Post post);
    void Remove(Post post);
}
