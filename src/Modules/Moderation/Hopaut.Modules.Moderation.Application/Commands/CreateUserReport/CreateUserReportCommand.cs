using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

/// <summary>
/// The caller reports a user (decision D-035): the reporter is the caller, never taken from the request. The reported user,
/// reason and message arrive as sent and the handler validates them.
/// </summary>
public sealed record CreateUserReportCommand(UserId ReportedUserId, int Reason, string? Message)
    : ICommand<Result<UserReportId>>, IModerationCommand;
