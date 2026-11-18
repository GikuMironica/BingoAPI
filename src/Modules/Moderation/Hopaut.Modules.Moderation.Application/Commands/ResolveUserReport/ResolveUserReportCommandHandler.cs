using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolveUserReport;

public sealed class ResolveUserReportCommandHandler(IUserReportRepository reports, ICurrentUser currentUser, TimeProvider timeProvider)
    : ICommandHandler<ResolveUserReportCommand, Result>
{
    public async ValueTask<Result> Handle(ResolveUserReportCommand request, CancellationToken cancellationToken)
    {
        var report = await reports.GetByIdAsync(request.ReportId, cancellationToken);
        return report is null
            ? UserReportErrors.NotFound(request.ReportId)
            : report.Resolve(currentUser.Id, timeProvider.GetUtcNow());
    }
}
