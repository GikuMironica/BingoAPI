using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.ResolvePostReport;

public sealed record ResolvePostReportCommand(PostReportId ReportId) : IRequest<bool>;
