using Hopaut.SharedKernel;

namespace Hopaut.Modules.Media.Application;

/// <summary>Expected failures of the image upload use case; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class MediaUploadErrors
{
    public static Error ProcessingFailed(string? reason) =>
        Error.Validation("media.image.processing_failed", reason ?? "The image could not be processed.");

    public static Error StorageFailed(string? reason) =>
        Error.Failure("media.image.storage_failed", reason ?? "The image could not be stored.");
}
