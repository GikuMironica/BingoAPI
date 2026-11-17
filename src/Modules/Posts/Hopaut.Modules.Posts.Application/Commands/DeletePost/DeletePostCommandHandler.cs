using Hopaut.BuildingBlocks.Application.Security;
using Hopaut.Modules.Posts.Application.Authorization;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed class DeletePostCommandHandler(IPostRepository posts, IResourceAuthorizer authorizer, TimeProvider timeProvider)
    : ICommandHandler<DeletePostCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await posts.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return PostErrors.NotFound(request.PostId);

        if (!await authorizer.IsAuthorizedAsync(post, PostPolicies.MustOwnPost, cancellationToken))
            return PostErrors.NotHost(request.PostId);

        post.Delete(timeProvider.GetUtcNow());
        posts.Remove(post);
        return Result.Success();
    }
}
