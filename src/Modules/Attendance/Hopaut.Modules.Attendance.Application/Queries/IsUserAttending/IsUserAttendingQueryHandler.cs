using MediatR;

namespace Hopaut.Modules.Attendance.Application.Queries.IsUserAttending;

public sealed class IsUserAttendingQueryHandler : IRequestHandler<IsUserAttendingQuery, bool>
{
    private readonly IAttendanceRepository _repo;

    public IsUserAttendingQueryHandler(IAttendanceRepository repo) => _repo = repo;

    public async Task<bool> Handle(IsUserAttendingQuery request, CancellationToken cancellationToken)
    {
        return await _repo.IsUserAttendingAsync(request.PostId, request.UserId, cancellationToken);
    }
}
