using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Expected failures of the Post aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class PostErrors
{
    public static Error NotFound(PostId postId) =>
        Error.NotFound("posts.post.not_found", $"Post {postId} was not found.");

    public static Error NotHost(PostId postId) =>
        Error.Forbidden("posts.post.not_host", $"Only the host of post {postId} can change it.");
}
