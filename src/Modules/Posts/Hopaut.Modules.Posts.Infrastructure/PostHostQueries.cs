using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Serves <see cref="IPostHostQueries"/> with no-tracking queries.</summary>
public sealed class PostHostQueries(PostsModuleDbContext db) : IPostHostQueries
{
    public Task<PostHost?> GetAsync(PostId postId, CancellationToken cancellationToken) =>
        db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new PostHost(p.Id, p.UserId))
            .SingleOrDefaultAsync(cancellationToken);

    // TODO: add index — the IX_posts_UserId_EventTime_Id proposed in PostReadModel.GetHostedAsync serves this filter too.
    public async Task<IReadOnlyList<PostId>> GetHostedPostIdsAsync(UserId hostId, CancellationToken cancellationToken) =>
        await db.Posts
            .AsNoTracking()
            .Where(p => p.UserId == hostId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
}
