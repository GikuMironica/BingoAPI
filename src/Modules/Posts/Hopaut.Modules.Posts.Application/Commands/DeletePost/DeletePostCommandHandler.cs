using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed class DeletePostCommandHandler(IPostRepository posts) : ICommandHandler<DeletePostCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await posts.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return PostErrors.NotFound(request.PostId);

        // Resource-based authorization moves to the authorization port in plan Step 6.9.
        if (post.UserId != request.UserId)
            return PostErrors.NotHost(request.PostId);

        posts.Remove(post);
        return Result.Success();
    }
}
