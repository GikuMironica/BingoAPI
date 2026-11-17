using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application.Authorization;

/// <summary>Authorizes the caller as the host of the post whose attendance is decided (<see cref="AttendancePolicies.MustHostPost"/>).</summary>
internal static class PostHostAuthorization
{
    public static async Task<Result> AuthorizePostHostAsync(
        this IResourceAuthorizer authorizer, IPostHostQueries posts, PostId postId, CancellationToken cancellationToken)
    {
        var host = await posts.GetAsync(postId, cancellationToken);
        if (host is null)
            return ParticipationErrors.PostNotFound(postId);

        return await authorizer.IsAuthorizedAsync(host, AttendancePolicies.MustHostPost, cancellationToken)
            ? Result.Success()
            : ParticipationErrors.NotPostHost(postId);
    }
}
