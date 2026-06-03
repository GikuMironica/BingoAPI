using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Moderation.Application.Commands.CreateUserReport;

public sealed record CreateUserReportCommand(UserId ReporterId, UserId ReportedUserId, int Reason, string? Message) : IRequest<UserReportId>;
