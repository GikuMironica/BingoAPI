using Hopaut.Modules.BugReports.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.BugReports.Application.Commands.CreateBugReport;

public sealed class CreateBugReportCommandHandler : IRequestHandler<CreateBugReportCommand, BugReportId>
{
    private readonly IBugReportRepository _repo;

    public CreateBugReportCommandHandler(IBugReportRepository repo) => _repo = repo;

    public async Task<BugReportId> Handle(CreateBugReportCommand request, CancellationToken cancellationToken)
    {
        var bug = Bug.Create(request.Message, request.ReporterId, request.ScreenshotUrls);

        await _repo.AddAsync(bug, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return bug.Id;
    }
}
