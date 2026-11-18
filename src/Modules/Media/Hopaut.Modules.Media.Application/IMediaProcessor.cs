namespace Hopaut.Modules.Media.Application;

/// <summary>
/// Processes raw uploaded images asynchronously (called by Hangfire).
/// Downloads from temp S3 path, resizes via ImageSharp, writes finals, updates picture state.
/// </summary>
public interface IMediaProcessor
{
    Task ProcessAsync(int postId, IReadOnlyList<string> tempKeys, CancellationToken ct = default);
}
