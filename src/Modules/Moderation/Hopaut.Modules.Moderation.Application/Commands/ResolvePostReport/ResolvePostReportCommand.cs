using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

public sealed record ResolvePostReportCommand(PostReportId ReportId) : ICommand<Result>, IModerationCommand;
