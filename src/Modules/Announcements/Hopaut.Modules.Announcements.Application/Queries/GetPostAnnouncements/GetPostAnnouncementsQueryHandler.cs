using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Announcements.Application.Authorization;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetPostAnnouncements;

public sealed class GetPostAnnouncementsQueryHandler(
    IAnnouncementReadModel readModel,
    IPostHostQueries posts,
    IAttendanceQueries attendance,
    IResourceAuthorizer authorizer,
    ICurrentUser currentUser) : IQueryHandler<GetPostAnnouncementsQuery, Result<AnnouncementPage>>
{
    public async ValueTask<Result<AnnouncementPage>> Handle(GetPostAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var after = request.Cursor is null ? null : AnnouncementKeyset.Decode(request.Cursor);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);

        if (ValidationError.FromFailures(("cursor", after), ("limit", pageSize)) is { } invalid)
            return invalid;

        var authorized = await authorizer.AuthorizeReaderAsync(posts, attendance, currentUser, request.PostId, cancellationToken);
        if (authorized.IsFailure)
            return authorized.Error;

        var announcements = await readModel.GetPageAsync(
            request.PostId, after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var items = announcements.Take(pageSize.Value.Value).ToList();
        var nextCursor = announcements.Count > items.Count ? AnnouncementKeyset.Of(items[^1]).Encode().Value : null;

        return new AnnouncementPage(items, nextCursor);
    }
}
