using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed class IsUserAttendingQueryHandler(IAttendanceRepository participations) : IQueryHandler<IsUserAttendingQuery, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(IsUserAttendingQuery request, CancellationToken cancellationToken) =>
        await participations.IsUserAttendingAsync(request.PostId, request.UserId, cancellationToken);
}
