using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed record CreateBugReportCommand(string Message, UserId ReporterId, List<string>? ScreenshotUrls)
    : ICommand<Result<BugReportId>>, IBugReportsCommand;
