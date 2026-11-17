using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Application.Queries;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed class IsUserAttendingQueryHandler(IAttendanceReadModel readModel, ICurrentUser currentUser)
    : IQueryHandler<IsUserAttendingQuery, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(IsUserAttendingQuery request, CancellationToken cancellationToken) =>
        await readModel.IsAcceptedAsync(request.PostId, currentUser.Id, cancellationToken);
}
