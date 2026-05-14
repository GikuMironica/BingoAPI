using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed record CreateUserReportCommand(string ReporterId, string ReportedUserId, int Reason, string? Message) : IRequest<int>;
