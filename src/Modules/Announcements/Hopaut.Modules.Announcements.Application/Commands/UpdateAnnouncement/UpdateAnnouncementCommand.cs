using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.UpdateAnnouncement;

/// <summary>
/// The post's host or an administrator replaces the message of an announcement (decision D-033). The message arrives as
/// sent and the handler validates it.
/// </summary>
public sealed record UpdateAnnouncementCommand(AnnouncementId AnnouncementId, string? Message) : ICommand<Result>, IAnnouncementsCommand;
