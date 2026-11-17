using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed class RequestAttendanceCommandHandler(IAttendanceRepository participations, TimeProvider timeProvider)
    : ICommandHandler<RequestAttendanceCommand, Result>
{
    public async ValueTask<Result> Handle(RequestAttendanceCommand request, CancellationToken cancellationToken)
    {
        var existing = await participations.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (existing is not null)
            return ParticipationErrors.AlreadyRequested(request.PostId, request.UserId);

        var id = await participations.NextIdAsync(cancellationToken);
        participations.Add(Participation.Create(id, request.PostId, request.UserId, timeProvider.GetUtcNow()));

        return Result.Success();
    }
}
