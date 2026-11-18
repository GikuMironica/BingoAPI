using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.DeleteAnnouncement;

public sealed class DeleteAnnouncementCommandHandler(
    IAnnouncementRepository announcements, IPostHostQueries posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<DeleteAnnouncementCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await announcements.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return AnnouncementErrors.NotFound(request.AnnouncementId);

        var authorized = await authorizer.AuthorizeAnnouncerAsync(posts, announcement.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized;

        announcement.Delete(timeProvider.GetUtcNow());
        announcements.Remove(announcement);
        return Result.Success();
    }
}
