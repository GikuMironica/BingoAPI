using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed class AcceptAttendanceCommandHandler(
    IAttendanceRepository participations, IPostHostQueries posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<AcceptAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(AcceptAttendanceCommand request, CancellationToken cancellationToken)
    {
        var authorized = await authorizer.AuthorizePostHostAsync(posts, request.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized;

        var participation = await participations.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(request.PostId, request.UserId);

        return participation.Accept(timeProvider.GetUtcNow())
            ? Result.Success()
            : ParticipationErrors.NotPending(participation.Id);
    }
}
