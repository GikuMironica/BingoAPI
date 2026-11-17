using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetNearbyPosts;

public sealed record GetNearbyPostsQuery(double Longitude, double Latitude, double RadiusMeters = 10000, int Limit = 50)
    : IQuery<Result<IReadOnlyList<PostDto>>>;
