using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed record CreateUserReportCommand(UserId ReporterId, UserId ReportedUserId, int Reason, string? Message)
    : ICommand<Result<UserReportId>>, IModerationCommand;
