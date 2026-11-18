using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed record GetPostByIdQuery(PostId PostId) : IQuery<Result<PostDto>>;
