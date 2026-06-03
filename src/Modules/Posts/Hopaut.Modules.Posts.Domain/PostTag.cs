using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

public sealed class PostTag
{
    public PostId PostId { get; set; }
    public Post? Post { get; set; }
    public TagId TagId { get; set; }
    public Tag? Tag { get; set; }
}
