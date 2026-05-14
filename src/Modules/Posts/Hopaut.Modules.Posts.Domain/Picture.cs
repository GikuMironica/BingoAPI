namespace Hopaut.Modules.Posts.Domain;

public sealed class Picture
{
    public int Id { get; set; }
    public string Url { get; set; } = default!;
    public string? TempKey { get; set; }
    public PictureState State { get; set; } = PictureState.Pending;
    public int PostId { get; set; }
    public Post? Post { get; set; }
}
