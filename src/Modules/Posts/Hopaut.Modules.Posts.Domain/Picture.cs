using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>A picture of a post: <see cref="PictureState.Pending"/> until processing marks it ready (with its final URL) or failed.</summary>
public sealed class Picture
{
    private Picture() { } // EF Core

    public PictureId Id { get; private set; }

    public PostId PostId { get; private set; }

    public string Url { get; private set; }

    public string? TempKey { get; private set; }

    public PictureState State { get; private set; }

    internal static Picture Create(PostId postId, string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        return new Picture { PostId = postId, Url = url, State = PictureState.Pending };
    }

    internal bool MarkReady(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        if (State != PictureState.Pending)
            return false;

        Url = url;
        TempKey = null;
        State = PictureState.Ready;
        return true;
    }

    internal bool MarkFailed()
    {
        if (State != PictureState.Pending)
            return false;

        State = PictureState.Failed;
        return true;
    }
}
