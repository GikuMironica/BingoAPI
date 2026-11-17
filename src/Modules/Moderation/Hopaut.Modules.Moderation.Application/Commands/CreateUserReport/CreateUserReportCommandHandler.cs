using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed class CreateUserReportCommandHandler(IModerationRepository reports, TimeProvider timeProvider)
    : ICommandHandler<CreateUserReportCommand, Result<UserReportId>>
{
    public async ValueTask<Result<UserReportId>> Handle(CreateUserReportCommand request, CancellationToken cancellationToken)
    {
        var id = await reports.NextUserReportIdAsync(cancellationToken);
        var report = UserReport.Create(
            id, request.Reason, request.Message,
            request.ReporterId, request.ReportedUserId,
            timeProvider.GetUtcNow());

        reports.AddUserReport(report);
        return report.Id;
    }
}
