using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolveUserReport;

/// <summary>The caller, an administrator, resolves an open user report (decision D-035); the endpoint requires the Admin policy.</summary>
public sealed record ResolveUserReportCommand(UserReportId ReportId) : ICommand<Result>, IModerationCommand;
