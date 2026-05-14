namespace Hopaut.Modules.Media.Domain;

public readonly record struct MediaAssetId(Guid Value)
{
    public static MediaAssetId New() => new(Guid.NewGuid());
}

/// <summary>
/// Represents a stored media asset (image, etc.) in cloud storage.
/// </summary>
public sealed class MediaAsset
{
    public MediaAssetId Id { get; private set; }
    public string FileName { get; private set; } = default!;
    public string BucketPath { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long SizeBytes { get; private set; }
    public string UploadedByUserId { get; private set; } = default!;
    public DateTime UploadedAtUtc { get; private set; }

    private MediaAsset() { }

    public static MediaAsset Create(string fileName, string bucketPath, string contentType, long sizeBytes, string uploadedByUserId)
    {
        return new MediaAsset
        {
            Id = MediaAssetId.New(),
            FileName = fileName,
            BucketPath = bucketPath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedByUserId = uploadedByUserId,
            UploadedAtUtc = DateTime.UtcNow
        };
    }
}
