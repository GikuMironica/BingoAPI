using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

/// <summary>The post's host or an administrator deletes an announcement (decision D-033).</summary>
public sealed record DeleteAnnouncementCommand(AnnouncementId AnnouncementId) : ICommand<Result>, IAnnouncementsCommand;
