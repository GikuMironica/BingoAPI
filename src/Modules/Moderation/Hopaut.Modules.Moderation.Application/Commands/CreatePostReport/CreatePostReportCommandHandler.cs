using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed class CreatePostReportCommandHandler(IModerationRepository reports, TimeProvider timeProvider)
    : ICommandHandler<CreatePostReportCommand, Result<PostReportId>>
{
    public async ValueTask<Result<PostReportId>> Handle(CreatePostReportCommand request, CancellationToken cancellationToken)
    {
        var id = await reports.NextPostReportIdAsync(cancellationToken);
        var report = PostReport.Create(
            id, request.Reason, request.Message,
            request.ReporterId, request.ReportedHostId, request.PostId,
            timeProvider.GetUtcNow());

        reports.AddPostReport(report);
        return report.Id;
    }
}
