using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Authorization;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed class RequestAttendanceCommandHandler(
    IAttendanceRepository participations,
    IPostAttendanceTermsQueries posts,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<RequestAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(RequestAttendanceCommand request, CancellationToken cancellationToken)
    {
        var terms = await posts.GetTermsAsync(request.PostId, cancellationToken);
        if (terms.IsFailure)
            return terms;

        var requester = currentUser.Id;
        var now = timeProvider.GetUtcNow();
        var acceptedAttendees = terms.Value.Slots is null ? 0 : await participations.CountAcceptedAsync(request.PostId, cancellationToken);

        var existing = await participations.GetAsync(request.PostId, requester, cancellationToken);
        if (existing is not null)
            return existing.RequestAgain(terms.Value, acceptedAttendees, now);

        var id = await participations.NextIdAsync(cancellationToken);
        var participation = Participation.Request(id, terms.Value, requester, acceptedAttendees, now);
        if (participation.IsFailure)
            return participation;

        participations.Add(participation.Value);
        return Result.Success();
    }
}
