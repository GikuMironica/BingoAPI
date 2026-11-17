using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.CreateAnnouncement;

public sealed class CreateAnnouncementCommandHandler(
    IAnnouncementRepository announcements, IPostHostQueries posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<CreateAnnouncementCommand, Result<AnnouncementId>>
{
    public async ValueTask<Result<AnnouncementId>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var authorized = await authorizer.AuthorizeAnnouncementOwnerAsync(posts, request.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized.Error;

        var id = await announcements.NextIdAsync(cancellationToken);
        var announcement = Announcement.Create(id, request.PostId, request.Message, timeProvider.GetUtcNow());

        announcements.Add(announcement);
        return announcement.Id;
    }
}
