using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

public sealed record CreatePostReportCommand(PostId PostId, UserId ReporterId, UserId ReportedHostId, int Reason, string? Message)
    : ICommand<Result<PostReportId>>, IModerationCommand;
