using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed record CreatePostReportCommand(int PostId, string ReporterId, string ReportedHostId, int Reason, string? Message) : IRequest<int>;
