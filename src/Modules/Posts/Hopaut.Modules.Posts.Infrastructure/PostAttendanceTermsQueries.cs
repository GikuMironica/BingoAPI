using Hopaut.Modules.Posts.Contracts;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>Serves <see cref="IPostAttendanceTermsQueries"/> with one no-tracking projection.</summary>
public sealed class PostAttendanceTermsQueries(PostsModuleDbContext db) : IPostAttendanceTermsQueries
{
    public async Task<PostAttendanceTerms?> GetAsync(PostId postId, CancellationToken cancellationToken)
    {
        var terms = await db.Posts
            .AsNoTracking()
            .Where(p => p.Id == postId)
            .Select(p => new { p.Id, p.UserId, p.Status, p.Schedule.End, p.Event.EventType, p.Event.Slots })
            .SingleOrDefaultAsync(cancellationToken);

        return terms is null
            ? null
            : new PostAttendanceTerms(
                new PostHost(terms.Id, terms.UserId),
                terms.Status == PostStatus.Active,
                terms.End,
                terms.EventType.HostApprovesAttendees(),
                terms.Slots?.Value);
    }
}
