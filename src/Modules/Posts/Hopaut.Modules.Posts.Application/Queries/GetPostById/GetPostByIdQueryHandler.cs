using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Queries.GetPostById;

public sealed class GetPostByIdQueryHandler(IPostReadModel readModel) : IQueryHandler<GetPostByIdQuery, Result<PostDto>>
{
    public async ValueTask<Result<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await readModel.GetByIdAsync(request.PostId, cancellationToken);
        return post is null ? PostErrors.NotFound(request.PostId) : post;
    }
}
