using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

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
            .Include(p => p.Tags)
            .AsSplitQuery()
            .SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Add(Post post) => _dbContext.Posts.Add(post);

    public void Remove(Post post) => _dbContext.Posts.Remove(post);
}
