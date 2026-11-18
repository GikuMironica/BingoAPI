using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Announcements.Domain;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementById;

public sealed class GetAnnouncementByIdQueryHandler(
    IAnnouncementReadModel readModel,
    IPostHostQueries posts,
    IAttendanceQueries attendance,
    IResourceAuthorizer authorizer,
    ICurrentUser currentUser) : IQueryHandler<GetAnnouncementByIdQuery, Result<AnnouncementDto>>
{
    public async ValueTask<Result<AnnouncementDto>> Handle(GetAnnouncementByIdQuery request, CancellationToken cancellationToken)
    {
        var announcement = await readModel.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return AnnouncementErrors.NotFound(request.AnnouncementId);

        var authorized = await authorizer.AuthorizeReaderAsync(posts, attendance, currentUser, announcement.PostId, cancellationToken);
        return authorized.IsSuccess ? announcement : authorized.Error;
    }
}
