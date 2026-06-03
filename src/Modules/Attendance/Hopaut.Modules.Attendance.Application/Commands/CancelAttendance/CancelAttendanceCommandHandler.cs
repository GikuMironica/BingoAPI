using Hopaut.Modules.Attendance.Domain;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.CancelAttendance;

public sealed class CancelAttendanceCommandHandler : IRequestHandler<CancelAttendanceCommand, bool>
{
    private readonly IAttendanceRepository _repo;

    public CancelAttendanceCommandHandler(IAttendanceRepository repo) => _repo = repo;

    public async Task<bool> Handle(CancelAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await _repo.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null) return false;

        if (!participation.Cancel()) return false;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
