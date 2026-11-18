using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed class CreateBugReportCommandHandler(IBugReportRepository bugs, TimeProvider timeProvider)
    : ICommandHandler<CreateBugReportCommand, Result<BugReportId>>
{
    public async ValueTask<Result<BugReportId>> Handle(CreateBugReportCommand request, CancellationToken cancellationToken)
    {
        var id = await bugs.NextIdAsync(cancellationToken);
        var bug = Bug.Create(id, request.Message, request.ReporterId, request.ScreenshotUrls, timeProvider.GetUtcNow());

        bugs.Add(bug);
        return bug.Id;
    }
}
