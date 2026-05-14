using MediatR;

namespace Hopaut.Modules.Posts.Application.Commands.DeletePost;

public sealed class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, bool>
{
    private readonly IPostRepository _repo;

    public DeletePostCommandHandler(IPostRepository repo) => _repo = repo;

    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null || post.UserId != request.UserId)
            return false;

        await _repo.DeleteAsync(post, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return true;
    }
}
