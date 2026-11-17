using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Authorization;

/// <summary>Authorizes the caller against the host of the post that owns the announcements (<see cref="AnnouncementPolicies.MustOwnAnnouncement"/>).</summary>
internal static class PostHostAuthorization
{
    public static async Task<Result> AuthorizeAnnouncementOwnerAsync(
        this IResourceAuthorizer authorizer, IPostHostQueries posts, PostId postId, CancellationToken cancellationToken)
    {
        var host = await posts.GetAsync(postId, cancellationToken);
        if (host is null)
            return AnnouncementErrors.PostNotFound(postId);

        return await authorizer.IsAuthorizedAsync(host, AnnouncementPolicies.MustOwnAnnouncement, cancellationToken)
            ? Result.Success()
            : AnnouncementErrors.NotPostHost(postId);
    }
}
