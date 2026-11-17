using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed class CreatePostReportCommandHandler : IRequestHandler<CreatePostReportCommand, PostReportId>
{
    private readonly IModerationRepository _repo;
    private readonly TimeProvider _timeProvider;

    public CreatePostReportCommandHandler(IModerationRepository repo, TimeProvider timeProvider)
    {
        _repo = repo;
        _timeProvider = timeProvider;
    }

    public async Task<PostReportId> Handle(CreatePostReportCommand request, CancellationToken cancellationToken)
    {
        var report = PostReport.Create(
            request.Reason, request.Message,
            request.ReporterId, request.ReportedHostId, request.PostId,
            _timeProvider.GetUtcNow());

        await _repo.AddPostReportAsync(report, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return report.Id;
    }
}
