namespace Hopaut.Modules.Media.Application;

/// <summary>
/// Port for image processing (resize, format conversion, etc.).
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Processes an image stream: resizes to max dimensions and converts to WebP.
    /// Returns the processed stream and content type.
    /// </summary>
    Task<ImageProcessResult> ProcessAsync(Stream input, int maxWidth = 1920, int maxHeight = 1080, CancellationToken ct = default);
}

public sealed record ImageProcessResult(bool Success, Stream? Output = null, string ContentType = "image/webp", string? Error = null);
