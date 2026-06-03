using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed record CreateBugReportCommand(string Message, UserId ReporterId, List<string>? ScreenshotUrls) : IRequest<BugReportId>;
