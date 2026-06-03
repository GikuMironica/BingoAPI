using Hopaut.Modules.Attendance.Domain;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RejectAttendance;

public sealed class RejectAttendanceCommandHandler : IRequestHandler<RejectAttendanceCommand, bool>
{
    private readonly IAttendanceRepository _repo;

    public RejectAttendanceCommandHandler(IAttendanceRepository repo) => _repo = repo;

    public async Task<bool> Handle(RejectAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await _repo.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null) return false;

        if (!participation.Reject()) return false;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
