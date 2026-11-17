using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetMyPosts;

/// <summary>
/// The caller's own posts, newest start first: <c>active</c> (default) or <c>inactive</c>, keyset-paged by an opaque
/// cursor; inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record GetMyPostsQuery(string? Status, string? Cursor, int? Limit) : IQuery<Result<PostPage>>;
