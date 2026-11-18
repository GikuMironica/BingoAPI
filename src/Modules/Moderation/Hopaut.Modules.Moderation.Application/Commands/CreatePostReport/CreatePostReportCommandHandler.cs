using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed class CreatePostReportCommandHandler(
    IPostReportRepository reports,
    IPostHostQueries posts,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<CreatePostReportCommand, Result<PostReportId>>
{
    public async ValueTask<Result<PostReportId>> Handle(CreatePostReportCommand request, CancellationToken cancellationToken)
    {
        var reason = ReportReason.Create(request.Reason);
        var message = ReportInput.Message(request.Message);

        if (ValidationError.FromFailures(("reason", reason), ("message", message)) is { } invalid)
            return invalid;

        var host = await posts.GetAsync(request.PostId, cancellationToken);
        if (host is null)
            return PostReportErrors.PostNotFound(request.PostId);

        var reporter = currentUser.Id;
        var post = new ReportedPost(
            host.PostId,
            host.HostId,
            ReporterHasOpenReport: await reports.HasOpenReportAsync(reporter, host.PostId, cancellationToken));

        var id = await reports.NextIdAsync(cancellationToken);
        var report = PostReport.Create(id, post, reporter, reason.Value, message?.Value, timeProvider.GetUtcNow());
        if (report.IsFailure)
            return report.Error;

        reports.Add(report.Value);
        return report.Value.Id;
    }
}
