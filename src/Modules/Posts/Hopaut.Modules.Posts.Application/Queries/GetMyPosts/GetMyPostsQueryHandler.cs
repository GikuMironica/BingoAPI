using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetMyPosts;

public sealed class GetMyPostsQueryHandler(
    IPostReadModel readModel,
    IUserReputationProvider reputationProvider,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IQueryHandler<GetMyPostsQuery, Result<PostPage>>
{
    public async ValueTask<Result<PostPage>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        var filter = PostListErrors.ParseFilter(request.Status);
        var after = request.Cursor is null ? null : PostKeyset.Decode(request.Cursor);
        var pageSize = request.Limit is { } limit ? PageSize.Create(limit) : Result.Success(PageSize.Default);

        var invalid = ValidationError.FromFailures(("status", filter), ("cursor", after), ("limit", pageSize));
        if (invalid is not null)
            return invalid;

        var host = currentUser.Id;
        var posts = await readModel.GetHostedAsync(
            host, filter.Value, timeProvider.GetUtcNow(), after?.Value, pageSize.Value.Value + 1, cancellationToken);

        var items = posts.Take(pageSize.Value.Value).ToList();
        var nextCursor = posts.Count > items.Count
            ? new PostKeyset(items[^1].EventTime, PostId.From(items[^1].Id)).Encode().Value
            : null;

        if (items.Count > 0)
        {
            var reputations = await reputationProvider.GetBatchAsync([host], cancellationToken);
            if (reputations.TryGetValue(host, out var reputation))
                items = items.ConvertAll(item => item with { UserReputation = reputation });
        }

        return new PostPage(items, nextCursor);
    }
}
