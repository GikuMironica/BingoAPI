using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(PostId PostId, string Message) : IRequest<AnnouncementId>;
