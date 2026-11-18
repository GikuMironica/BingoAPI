namespace Hopaut.Modules.Posts.Application.Authorization;

/// <summary>Resource-based policies of the Posts module (plan Step 6.9), registered by <c>AddPostsModule</c>.</summary>
public static class PostPolicies
{
    /// <summary>The caller hosts the post, or is an administrator (the legacy <c>IsPostOwnerOrAdminAsync</c> rule).</summary>
    public const string MustOwnPost = "posts.post.owner";
}
