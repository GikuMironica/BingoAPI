using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(AnnouncementId AnnouncementId) : IRequest<bool>;
