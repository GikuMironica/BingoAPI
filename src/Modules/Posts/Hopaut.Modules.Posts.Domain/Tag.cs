namespace Hopaut.Modules.Posts.Domain;

public sealed class Tag
{
    public int Id { get; set; }
    public string TagName { get; set; } = default!;
    public List<PostTag> Posts { get; set; } = [];
}
