using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

/// <summary>The caller, an administrator, resolves an open post report (decision D-035); the endpoints require the Admin policy (plan Step 6.9).</summary>
public sealed record ResolvePostReportCommand(PostReportId ReportId) : ICommand<Result>, IModerationCommand;
