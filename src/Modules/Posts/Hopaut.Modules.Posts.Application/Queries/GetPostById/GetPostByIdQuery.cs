using Hopaut.SharedKernel;
using MediatR;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed record GetPostByIdQuery(PostId PostId) : IRequest<PostDto?>;
