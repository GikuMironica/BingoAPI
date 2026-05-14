using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(int AnnouncementId) : IRequest<bool>;
