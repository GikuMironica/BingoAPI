using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

public sealed class Picture
{
    public PictureId Id { get; set; }
    public string Url { get; set; } = default!;
    public string? TempKey { get; set; }
    public PictureState State { get; set; } = PictureState.Pending;
    public PostId PostId { get; set; }
    public Post? Post { get; set; }
}
