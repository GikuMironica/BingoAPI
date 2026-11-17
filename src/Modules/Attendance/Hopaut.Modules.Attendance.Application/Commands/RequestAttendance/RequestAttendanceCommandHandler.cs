using Hopaut.Modules.Attendance.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Attendance.Application.Commands.RequestAttendance;

public sealed class RequestAttendanceCommandHandler : IRequestHandler<RequestAttendanceCommand, bool>
{
    private readonly IAttendanceRepository _repo;
    private readonly TimeProvider _timeProvider;

    public RequestAttendanceCommandHandler(IAttendanceRepository repo, TimeProvider timeProvider)
    {
        _repo = repo;
        _timeProvider = timeProvider;
    }

    public async Task<bool> Handle(RequestAttendanceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetAsync(request.PostId, request.UserId, cancellationToken);
        if (existing is not null) return false;

        await _repo.AddAsync(Participation.Create(request.PostId, request.UserId, _timeProvider.GetUtcNow()), cancellationToken);

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
