using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Authorization;

/// <summary>
/// Who may act on a post's announcements (decision D-033, the legacy rules): the post's host or an administrator announce,
/// edit and delete (<see cref="AnnouncementPolicies.MustOwnAnnouncement"/>); they and the post's accepted attendees read.
/// The host comes from Posts.Contracts, the attendance from Attendance.Contracts.
/// </summary>
internal static class AnnouncementAuthorization
{
    public static async Task<Result> AuthorizeAnnouncerAsync(
        this IResourceAuthorizer authorizer, IPostHostQueries posts, PostId postId, CancellationToken cancellationToken)
    {
        var host = await posts.GetAsync(postId, cancellationToken);
        if (host is null)
            return AnnouncementErrors.PostNotFound(postId);

        return await authorizer.IsAuthorizedAsync(host, AnnouncementPolicies.MustOwnAnnouncement, cancellationToken)
            ? Result.Success()
            : AnnouncementErrors.NotPostHost(postId);
    }

    public static async Task<Result> AuthorizeReaderAsync(
        this IResourceAuthorizer authorizer,
        IPostHostQueries posts,
        IAttendanceQueries attendance,
        ICurrentUser caller,
        PostId postId,
        CancellationToken cancellationToken)
    {
        var host = await posts.GetAsync(postId, cancellationToken);
        if (host is null)
            return AnnouncementErrors.PostNotFound(postId);

        if (await authorizer.IsAuthorizedAsync(host, AnnouncementPolicies.MustOwnAnnouncement, cancellationToken))
            return Result.Success();

        return await attendance.IsAcceptedAsync(postId, caller.Id, cancellationToken)
            ? Result.Success()
            : AnnouncementErrors.NotPostAudience(postId);
    }
}
