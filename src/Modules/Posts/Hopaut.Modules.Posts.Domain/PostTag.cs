using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Links a post to a tag (many-to-many join).</summary>
public sealed class PostTag
{
    private PostTag() { } // EF Core

    public PostId PostId { get; private set; }

    public TagId TagId { get; private set; }

    // TODO (F-020, plan Step 8.2 batch 4): the read model joins tag names by TagId; then this cross-aggregate navigation goes.
    public Tag? Tag { get; private set; }
}
