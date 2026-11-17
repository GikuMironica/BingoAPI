using Hopaut.Modules.Announcements.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

// TODO (F-011, plan Step 8.5): any user can delete any announcement; only the post host may.
public sealed class DeleteAnnouncementCommandHandler(IAnnouncementRepository announcements)
    : ICommandHandler<DeleteAnnouncementCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await announcements.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return AnnouncementErrors.NotFound(request.AnnouncementId);

        announcements.Remove(announcement);
        return Result.Success();
    }
}
