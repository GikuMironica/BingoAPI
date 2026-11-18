using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementsByPost;

public sealed class GetAnnouncementsByPostQueryHandler(
    IAnnouncementReadModel readModel,
    IPostHostQueries posts,
    IAttendanceQueries attendance,
    IResourceAuthorizer authorizer,
    ICurrentUser currentUser) : IQueryHandler<GetAnnouncementsByPostQuery, Result<IReadOnlyList<AnnouncementDto>>>
{
    public async ValueTask<Result<IReadOnlyList<AnnouncementDto>>> Handle(GetAnnouncementsByPostQuery request, CancellationToken cancellationToken)
    {
        var authorized = await authorizer.AuthorizeReaderAsync(posts, attendance, currentUser, request.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized.Error;

        return Result.Success(await readModel.GetByPostAsync(request.PostId, cancellationToken));
    }
}
