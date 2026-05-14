using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(int PostId, string Message) : IRequest<int>;
