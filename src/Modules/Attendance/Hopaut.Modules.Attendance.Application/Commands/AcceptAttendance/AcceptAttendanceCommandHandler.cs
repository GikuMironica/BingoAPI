using Hopaut.Modules.Attendance.Domain;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed class AcceptAttendanceCommandHandler : IRequestHandler<AcceptAttendanceCommand, bool>
{
    private readonly IAttendanceRepository _repo;

    public AcceptAttendanceCommandHandler(IAttendanceRepository repo) => _repo = repo;

    public async Task<bool> Handle(AcceptAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await _repo.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null || participation.Status != AttendanceStatus.Pending) return false;

        participation.Status = AttendanceStatus.Accepted;
        participation.RespondedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
