using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

public sealed class Tag
{
    public TagId Id { get; set; }
    public string TagName { get; set; } = default!;
    public List<PostTag> Posts { get; set; } = [];
}
