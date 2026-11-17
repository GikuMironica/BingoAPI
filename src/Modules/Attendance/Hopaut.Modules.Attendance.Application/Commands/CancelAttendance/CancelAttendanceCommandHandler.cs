using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed class CancelAttendanceCommandHandler(IAttendanceRepository participations, TimeProvider timeProvider)
    : ICommandHandler<CancelAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(CancelAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await participations.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null)
            return ParticipationErrors.NotFound(request.PostId, request.UserId);

        return participation.Cancel(timeProvider.GetUtcNow())
            ? Result.Success()
            : ParticipationErrors.AlreadyCancelled(participation.Id);
    }
}
