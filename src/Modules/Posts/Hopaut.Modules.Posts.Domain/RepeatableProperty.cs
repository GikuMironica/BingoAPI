namespace Hopaut.Modules.Posts.Domain;

public sealed class RepeatableProperty
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    // Repeatable-specific fields will be added as needed
}
