using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Commands.UpdateAnnouncement;

public sealed class UpdateAnnouncementCommandHandler(
    IAnnouncementRepository announcements, IPostHostQueries posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<UpdateAnnouncementCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var message = AnnouncementMessage.Create(request.Message);
        if (ValidationError.FromFailures(("message", message)) is { } invalid)
            return invalid;

        var announcement = await announcements.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return AnnouncementErrors.NotFound(request.AnnouncementId);

        var authorized = await authorizer.AuthorizeAnnouncerAsync(posts, announcement.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized;

        announcement.Edit(message.Value, timeProvider.GetUtcNow());
        return Result.Success();
    }
}
