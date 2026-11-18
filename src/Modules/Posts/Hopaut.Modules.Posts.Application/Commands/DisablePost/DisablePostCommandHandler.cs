using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DisablePost;

public sealed class DisablePostCommandHandler(IPostRepository posts) : ICommandHandler<DisablePostCommand, Result>
{
    public async ValueTask<Result> Handle(DisablePostCommand request, CancellationToken cancellationToken)
    {
        var post = await posts.GetByIdAsync(request.PostId, cancellationToken);
        return post is null ? PostErrors.NotFound(request.PostId) : post.Disable();
    }
}
