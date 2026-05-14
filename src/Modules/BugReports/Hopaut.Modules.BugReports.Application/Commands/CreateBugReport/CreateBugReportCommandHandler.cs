using Hopaut.Modules.BugReports.Domain;
using MediatR;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed class CreateBugReportCommandHandler : IRequestHandler<CreateBugReportCommand, int>
{
    private readonly IBugReportRepository _repo;

    public CreateBugReportCommandHandler(IBugReportRepository repo) => _repo = repo;

    public async Task<int> Handle(CreateBugReportCommand request, CancellationToken cancellationToken)
    {
        var bug = new Bug
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Message = request.Message,
            ReporterId = request.ReporterId
        };

        if (request.ScreenshotUrls is { Count: > 0 })
        {
            bug.Screenshots = request.ScreenshotUrls.Select(url => new BugScreenshot { Url = url }).ToList();
        }

        await _repo.AddAsync(bug, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return bug.Id;
    }
}
