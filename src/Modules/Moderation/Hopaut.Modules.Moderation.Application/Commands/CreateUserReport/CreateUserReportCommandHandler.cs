using Hopaut.Modules.Moderation.Domain;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed class CreateUserReportCommandHandler : IRequestHandler<CreateUserReportCommand, int>
{
    private readonly IModerationRepository _repo;

    public CreateUserReportCommandHandler(IModerationRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateUserReportCommand request, CancellationToken cancellationToken)
    {
        var report = new UserReport
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Reason = request.Reason,
            Message = request.Message,
            ReporterId = request.ReporterId,
            ReportedUserId = request.ReportedUserId
        };

        await _repo.AddUserReportAsync(report, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return report.Id;
    }
}
