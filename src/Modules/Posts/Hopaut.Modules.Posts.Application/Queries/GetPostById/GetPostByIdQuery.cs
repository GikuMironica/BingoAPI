using MediatR;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed record GetPostByIdQuery(int PostId) : IRequest<PostDto?>;
