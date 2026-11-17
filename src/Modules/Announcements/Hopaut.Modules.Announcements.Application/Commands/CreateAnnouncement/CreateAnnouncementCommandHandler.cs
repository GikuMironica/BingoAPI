using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler(IAnnouncementRepository announcements, TimeProvider timeProvider)
    : ICommandHandler<CreateAnnouncementCommand, Result<AnnouncementId>>
{
    public async ValueTask<Result<AnnouncementId>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var id = await announcements.NextIdAsync(cancellationToken);
        var announcement = Announcement.Create(id, request.PostId, request.Message, timeProvider.GetUtcNow());

        announcements.Add(announcement);
        return announcement.Id;
    }
}
