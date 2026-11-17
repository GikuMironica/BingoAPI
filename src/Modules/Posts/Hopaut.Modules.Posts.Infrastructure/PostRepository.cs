using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Hopaut.Modules.Posts.Infrastructure;

public sealed class PostRepository : IPostRepository
{
    private readonly PostsModuleDbContext _dbContext;

    public PostRepository(PostsModuleDbContext dbContext) => _dbContext = dbContext;

    public async Task<PostId> NextIdAsync(CancellationToken ct = default) =>
        new(await _dbContext.Database.NextSequenceValueAsync(PostsModuleDbContext.SchemaName, PostsModuleDbContext.PostIdSequence, ct));

    public async Task<Post?> GetByIdAsync(PostId id, CancellationToken ct = default) =>
        await _dbContext.Posts
            .Include(p => p.Location)
            .Include(p => p.Event)
            .Include(p => p.Pictures)
            .Include(p => p.Tags).ThenInclude(t => t.Tag)
            .SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Post>> GetNearbyAsync(double longitude, double latitude, double radiusMeters, int limit, CancellationToken ct = default)
    {
        var point = new Point(longitude, latitude) { SRID = 4326 };

        return await _dbContext.Posts
            .Include(p => p.Location)
            .Include(p => p.Event)
            .Include(p => p.Pictures)
            .Include(p => p.Tags).ThenInclude(t => t.Tag)
            .Where(p => p.Status == PostStatus.Active && p.Location.Location.IsWithinDistance(point, radiusMeters))
            .OrderBy(p => p.Location.Location.Distance(point))
            .Take(limit)
            .ToListAsync(ct);
    }

    public void Add(Post post) => _dbContext.Posts.Add(post);

    public void Remove(Post post) => _dbContext.Posts.Remove(post);
}
