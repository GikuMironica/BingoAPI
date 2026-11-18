using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

/// <summary>
/// The post's host or an administrator announces on the post (decision D-033). The message arrives as sent and the handler
/// validates it.
/// </summary>
public sealed record CreateAnnouncementCommand(PostId PostId, string? Message) : ICommand<Result<AnnouncementId>>, IAnnouncementsCommand;
