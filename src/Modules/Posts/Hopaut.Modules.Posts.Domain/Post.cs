namespace Hopaut.Modules.Posts.Domain;

public sealed class Post
{
    public int Id { get; set; }
    public long PostTime { get; set; }
    public long EventTime { get; set; }
    public long? EndTime { get; set; }
    public int ActiveFlag { get; set; }
    public string UserId { get; set; } = default!;

    public EventLocation Location { get; set; } = default!;
    public Event Event { get; set; } = default!;
    public List<Picture> Pictures { get; set; } = [];
    public List<PostTag> Tags { get; set; } = [];
    public RepeatableProperty? Repeatable { get; set; }
}
