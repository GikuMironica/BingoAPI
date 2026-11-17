using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

/// <summary>Resolves a post report; the endpoint requires the Admin policy (plan Step 6.9).</summary>
public sealed class ResolvePostReportCommandHandler(IModerationRepository reports) : ICommandHandler<ResolvePostReportCommand, Result>
{
    public async ValueTask<Result> Handle(ResolvePostReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reports.GetPostReportByIdAsync(request.ReportId, cancellationToken);
        if (report is null)
            return PostReportErrors.NotFound(request.ReportId);

        return report.Resolve()
            ? Result.Success()
            : PostReportErrors.NotOpen(request.ReportId);
    }
}
