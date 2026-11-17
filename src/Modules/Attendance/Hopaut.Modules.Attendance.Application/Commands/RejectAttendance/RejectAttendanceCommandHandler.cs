using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

public sealed class RejectAttendanceCommandHandler(
    IAttendanceRepository participations, IPostAttendanceTermsQueries posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<RejectAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(RejectAttendanceCommand request, CancellationToken cancellationToken)
    {
        var terms = await authorizer.GetTermsOfHostedPostAsync(posts, request.PostId, cancellationToken);
        if (terms.IsFailure)
            return terms;

        var participation = await participations.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(request.PostId, request.UserId);

        return participation.Reject(timeProvider.GetUtcNow());
    }
}
