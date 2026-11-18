using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

public sealed class ResolvePostReportCommandHandler(IPostReportRepository reports, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<ResolvePostReportCommand, Result>
{
    public async ValueTask<Result> Handle(ResolvePostReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reports.GetByIdAsync(request.ReportId, cancellationToken);
        return report is null
            ? PostReportErrors.NotFound(request.ReportId)
            : report.Resolve(currentUser.Id, timeProvider.GetUtcNow());
    }
}
