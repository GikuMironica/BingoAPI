using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed class GetPostByIdQueryHandler(IPostRepository posts) : IQueryHandler<GetPostByIdQuery, Result<PostDto>>
{
    public async ValueTask<Result<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await posts.GetByIdAsync(request.PostId, cancellationToken);
        return post is null ? PostErrors.NotFound(request.PostId) : PostDto.From(post);
    }
}
