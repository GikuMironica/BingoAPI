using System.Diagnostics.CodeAnalysis;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Media.Domain;

public readonly record struct MediaAssetId(Guid Value) : IStronglyTypedId<MediaAssetId, Guid>
{
    public static MediaAssetId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public static MediaAssetId From(Guid value) => new(value);

    public static MediaAssetId Parse(string s, IFormatProvider? provider) => StronglyTypedId.Parse<MediaAssetId>(s, provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out MediaAssetId result) =>
        StronglyTypedId.TryParseGuid(s, out result);

    public static bool TryParse([NotNullWhen(true)] string? s, out MediaAssetId result) => StronglyTypedId.TryParseGuid(s, out result);

    public override string ToString() => Value.ToString();
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
    public DateTimeOffset UploadedAtUtc { get; private set; }

    private MediaAsset() { }

    public static MediaAsset Create(string fileName, string bucketPath, string contentType, long sizeBytes, string uploadedByUserId, DateTimeOffset now)
    {
        return new MediaAsset
        {
            Id = MediaAssetId.New(now),
            FileName = fileName,
            BucketPath = bucketPath,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedByUserId = uploadedByUserId,
            UploadedAtUtc = now
        };
    }
}
