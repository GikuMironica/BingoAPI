using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Attendance.Application.Authorization;

/// <summary>Reads a post's attendance terms from Posts.Contracts and, for host-only decisions, authorizes the caller (<see cref="AttendancePolicies.MustHostPost"/>).</summary>
internal static class AttendanceTermsLookup
{
    public static async Task<Result<AttendanceTerms>> GetTermsAsync(
        this IPostAttendanceTermsQueries posts, PostId postId, CancellationToken cancellationToken)
    {
        var terms = await posts.GetAsync(postId, cancellationToken);
        return terms is null ? ParticipationErrors.PostNotFound(postId) : ToDomain(terms);
    }

    public static async Task<Result<AttendanceTerms>> GetTermsOfHostedPostAsync(
        this IResourceAuthorizer authorizer, IPostAttendanceTermsQueries posts, PostId postId, CancellationToken cancellationToken)
    {
        var terms = await posts.GetAsync(postId, cancellationToken);
        if (terms is null)
            return ParticipationErrors.PostNotFound(postId);

        return await authorizer.IsAuthorizedAsync(terms.Host, AttendancePolicies.MustHostPost, cancellationToken)
            ? ToDomain(terms)
            : ParticipationErrors.NotPostHost(postId);
    }

    // Posts stores slots as a Capacity, so a stored count is always valid.
    private static AttendanceTerms ToDomain(PostAttendanceTerms terms) => new(
        terms.Host.PostId,
        terms.Host.HostId,
        terms.IsListed,
        terms.EndsAt,
        terms.HostApprovesAttendees,
        terms.Slots is { } slots ? Capacity.Create(slots).Value : null);
}
