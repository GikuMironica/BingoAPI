using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed class CancelAttendanceCommandHandler(IAttendanceRepository participations, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<CancelAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(CancelAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participant = currentUser.Id;
        var participation = await participations.GetAsync(request.PostId, participant, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(request.PostId, participant);

        return participation.Cancel(timeProvider.GetUtcNow());
    }
}
