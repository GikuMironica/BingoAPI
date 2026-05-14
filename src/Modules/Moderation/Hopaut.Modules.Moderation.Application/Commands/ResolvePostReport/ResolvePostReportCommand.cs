using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

public sealed record ResolvePostReportCommand(int ReportId) : IRequest<bool>;
