using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(PostId PostId, string Message) : ICommand<Result<AnnouncementId>>, IAnnouncementsCommand;
