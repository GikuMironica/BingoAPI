using Hopaut.Modules.Attendance.Domain;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.AcceptAttendance;

public sealed class AcceptAttendanceCommandHandler : IRequestHandler<AcceptAttendanceCommand, bool>
{
    private readonly IAttendanceRepository _repo;
    private readonly TimeProvider _timeProvider;

    public AcceptAttendanceCommandHandler(IAttendanceRepository repo, TimeProvider timeProvider)
    {
        _repo = repo;
        _timeProvider = timeProvider;
    }

    public async Task<bool> Handle(AcceptAttendanceCommand request, CancellationToken cancellationToken)
    {
        var participation = await _repo.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (participation is null) return false;

        if (!participation.Accept(_timeProvider.GetUtcNow())) return false;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
