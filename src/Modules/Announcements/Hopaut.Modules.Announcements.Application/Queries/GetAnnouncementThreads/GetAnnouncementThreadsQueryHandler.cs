using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.Modules.Posts.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Announcements.Application.Queries.GetAnnouncementThreads;

public sealed class GetAnnouncementThreadsQueryHandler(
    IAnnouncementReadModel readModel,
    IAttendanceQueries attendance,
    IPostHostQueries posts,
    ICurrentUser currentUser) : IQueryHandler<GetAnnouncementThreadsQuery, Result<AnnouncementThreadPage>>
{
    public async ValueTask<Result<AnnouncementThreadPage>> Handle(GetAnnouncementThreadsQuery request, CancellationToken cancellationToken)
    {
        var box = AnnouncementThreads.ParseBox(request.Box);
        var after = request.Cursor is null ? null : AnnouncementThreadKeyset.Decode(request.Cursor);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);

        if (ValidationError.FromFailures(("box", box), ("cursor", after), ("limit", pageSize)) is { } invalid)
            return invalid;

        var postIds = box.Value == AnnouncementBox.Inbox
            ? await attendance.GetAcceptedPostIdsAsync(currentUser.Id, cancellationToken)
            : await posts.GetHostedPostIdsAsync(currentUser.Id, cancellationToken);

        var threads = await readModel.GetThreadsAsync(postIds, after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var items = threads.Take(pageSize.Value.Value).ToList();
        var nextCursor = threads.Count > items.Count ? AnnouncementThreadKeyset.Of(items[^1]).Encode().Value : null;

        return new AnnouncementThreadPage(items, nextCursor);
    }
}
