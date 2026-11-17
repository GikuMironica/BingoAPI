using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Serves <see cref="IPostHostQueries"/> with one no-tracking query.</summary>
public sealed class PostHostQueries(PostsModuleDbContext db) : IPostHostQueries
{
    public Task<PostHost?> GetAsync(PostId postId, CancellationToken cancellationToken) =>
        db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new PostHost(p.Id, p.UserId))
            .SingleOrDefaultAsync(cancellationToken);
}
