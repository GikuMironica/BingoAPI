using Hopaut.Modules.Moderation.Domain;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed class CreatePostReportCommandHandler : IRequestHandler<CreatePostReportCommand, int>
{
    private readonly IModerationRepository _repo;

    public CreatePostReportCommandHandler(IModerationRepository repo) => _repo = repo;

    public async Task<int> Handle(CreatePostReportCommand request, CancellationToken cancellationToken)
    {
        var report = new PostReport
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Reason = request.Reason,
            Message = request.Message,
            ReporterId = request.ReporterId,
            ReportedHostId = request.ReportedHostId,
            PostId = request.PostId
        };

        await _repo.AddPostReportAsync(report, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return report.Id;
    }
}
