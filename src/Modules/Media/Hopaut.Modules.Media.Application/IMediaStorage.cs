namespace Hopaut.Modules.Media.Application;

/// <summary>
/// Port for uploading media to cloud storage (S3, etc.).
/// </summary>
public interface IMediaStorage
{
    Task<MediaUploadResult> UploadAsync(Stream content, string bucketPath, string fileName, string contentType, CancellationToken ct = default);
    Task<bool> DeleteAsync(IEnumerable<string> keys, CancellationToken ct = default);
}

public sealed record MediaUploadResult(bool Success, string? Key = null, string? PublicUrl = null, string? Error = null);
