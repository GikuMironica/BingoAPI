using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(AnnouncementId AnnouncementId) : ICommand<Result>, IAnnouncementsCommand;
