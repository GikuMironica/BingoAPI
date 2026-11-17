using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Contracts;

/// <summary>Read-only lookup of a post's host, offered to modules that authorize host-only actions (plan Step 6.9).</summary>
public interface IPostHostQueries
{
    /// <summary>The host of the post, or null when the post does not exist.</summary>
    Task<PostHost?> GetAsync(PostId postId, CancellationToken cancellationToken);
}

/// <summary>A post and the user who hosts it; the resource of other modules' host policies.</summary>
public sealed record PostHost(PostId PostId, UserId HostId);
