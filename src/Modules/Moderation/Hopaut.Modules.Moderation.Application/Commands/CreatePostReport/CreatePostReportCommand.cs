using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Moderation.Application.Commands.CreatePostReport;

/// <summary>
/// The caller reports a post (decision D-035): the reporter is the caller and the reported host the post's host, neither is
/// taken from the request. Reason and message arrive as sent and the handler validates them.
/// </summary>
public sealed record CreatePostReportCommand(PostId PostId, int Reason, string? Message)
    : ICommand<Result<PostReportId>>, IModerationCommand;
