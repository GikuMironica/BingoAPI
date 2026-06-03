using Hopaut.Modules.Moderation.Domain;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

public sealed class ResolvePostReportCommandHandler : IRequestHandler<ResolvePostReportCommand, bool>
{
    private readonly IModerationRepository _repo;

    public ResolvePostReportCommandHandler(IModerationRepository repo) => _repo = repo;

    public async Task<bool> Handle(ResolvePostReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _repo.GetPostReportByIdAsync(request.ReportId, cancellationToken);
        if (report is null) return false;

        if (!report.Resolve()) return false;

        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
