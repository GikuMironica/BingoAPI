using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

/// <summary>
/// Active, not yet ended posts around a point, nearest first: radius 1–15 km (default 10 km), page size 1–100 (default
/// 20), keyset-paged by an opaque cursor; inputs arrive as sent and the handler validates them.
/// </summary>
public sealed record GetNearbyPostsQuery(double Longitude, double Latitude, double? RadiusMeters = null, int? Limit = null, string? Cursor = null)
    : IQuery<Result<PostPage>>;
