using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Links a post to a tag (many-to-many join); created and removed only by <see cref="Post"/>, which references the tag by ID only.</summary>
public sealed class PostTag
{
    private PostTag() { } // EF Core

    public PostId PostId { get; private set; }

    public TagId TagId { get; private set; }

    internal static PostTag Create(PostId postId, TagId tagId) => new() { PostId = postId, TagId = tagId };
}
