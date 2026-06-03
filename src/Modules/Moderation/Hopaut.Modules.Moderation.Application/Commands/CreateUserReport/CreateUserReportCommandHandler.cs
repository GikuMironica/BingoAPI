using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed class CreateUserReportCommandHandler : IRequestHandler<CreateUserReportCommand, UserReportId>
{
    private readonly IModerationRepository _repo;

    public CreateUserReportCommandHandler(IModerationRepository repo) => _repo = repo;

    public async Task<UserReportId> Handle(CreateUserReportCommand request, CancellationToken cancellationToken)
    {
        var report = UserReport.Create(
            request.Reason, request.Message,
            request.ReporterId, request.ReportedUserId);

        await _repo.AddUserReportAsync(report, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return report.Id;
    }
}
