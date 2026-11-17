using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Attendance.Contracts;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetMyAttendances;

public sealed class GetMyAttendancesQueryHandler(
    IPostReadModel readModel,
    IAttendanceQueries attendance,
    IUserReputationProvider reputationProvider,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IQueryHandler<GetMyAttendancesQuery, Result<PostPage>>
{
    public async ValueTask<Result<PostPage>> Handle(GetMyAttendancesQuery request, CancellationToken cancellationToken)
    {
        var filter = PostListErrors.ParseFilter(request.Status);
        var after = request.Cursor is null ? null : PostKeyset.Decode(request.Cursor);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);

        var invalid = ValidationError.FromFailures(("status", filter), ("cursor", after), ("limit", pageSize));
        if (invalid is not null)
            return invalid;

        var attended = await attendance.GetAcceptedPostIdsAsync(currentUser.Id, cancellationToken);
        var posts = await readModel.GetAttendedAsync(
            attended, filter.Value, timeProvider.GetUtcNow(), after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var items = posts.Take(pageSize.Value.Value).ToList();
        var nextCursor = posts.Count > items.Count
            ? new PostKeyset(items[^1].EventTime, PostId.From(items[^1].Id)).Encode().Value
            : null;

        // One batched reputation lookup for all hosts (no N+1).
        var hosts = items.Select(item => UserId.From(item.UserId)).Distinct().ToList();
        var reputations = hosts.Count == 0 ? [] : await reputationProvider.GetBatchAsync(hosts, cancellationToken);

        return new PostPage(
            items.ConvertAll(item => item with { UserReputation = reputations.GetValueOrDefault(UserId.From(item.UserId)) }),
            nextCursor);
    }
}
