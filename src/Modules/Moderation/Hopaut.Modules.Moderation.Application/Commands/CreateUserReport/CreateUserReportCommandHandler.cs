using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

/// <summary>
/// Files the caller's report on a user. The legacy API answered 404 for an unknown user; no module offers a user read port
/// yet, so that check waits for plan Step 8.8 (finding F-078).
/// </summary>
public sealed class CreateUserReportCommandHandler(
    IUserReportRepository reports,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : ICommandHandler<CreateUserReportCommand, Result<UserReportId>>
{
    public async ValueTask<Result<UserReportId>> Handle(CreateUserReportCommand request, CancellationToken cancellationToken)
    {
        var reported = ReportedUser.IsUserId(request.ReportedUserId)
            ? Result.Success(request.ReportedUserId)
            : Result.Failure<UserId>(UserReportErrors.InvalidReportedUser);
        var reason = ReportReason.Create(request.Reason);
        var message = ReportInput.Message(request.Message);

        if (ValidationError.FromFailures(("reportedUserId", reported), ("reason", reason), ("message", message)) is { } invalid)
            return invalid;

        var reporter = currentUser.Id;
        var user = new ReportedUser(
            reported.Value,
            ReporterLastOpenReportAt: await reports.GetLatestOpenReportTimeAsync(reporter, reported.Value, cancellationToken));

        var id = await reports.NextIdAsync(cancellationToken);
        var report = UserReport.Create(id, user, reporter, reason.Value, message?.Value, timeProvider.GetUtcNow());
        if (report.IsFailure)
            return report.Error;

        reports.Add(report.Value);
        return report.Value.Id;
    }
}
