using MediatR;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed record CreateBugReportCommand(string Message, string ReporterId, List<string>? ScreenshotUrls) : IRequest<int>;
