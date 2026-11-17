using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

// TODO (F-009, plan Step 8.3): OwnerId is not checked against the post host.
public sealed class RejectAttendanceCommandHandler(IAttendanceRepository participations, TimeProvider timeProvider)
    : ICommandHandler<RejectAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(RejectAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await participations.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(request.PostId, request.UserId);

        return participation.Reject(timeProvider.GetUtcNow())
            ? Result.Success()
            : ParticipationErrors.NotPending(participation.Id);
    }
}
