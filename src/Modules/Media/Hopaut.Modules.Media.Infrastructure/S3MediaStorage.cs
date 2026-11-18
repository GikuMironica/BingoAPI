using Amazon.S3;
using Amazon.S3.Model;
using Hopaut.Modules.Media.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hopaut.Modules.Media.Infrastructure;

public sealed class S3MediaStorageOptions
{
    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = "eu-central-1";
    public string AccessKeyId { get; set; } = default!;
    public string SecretAccessKey { get; set; } = default!;
    public string ContentFormat { get; set; } = "image/webp";
}

public sealed class S3MediaStorage : IMediaStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3MediaStorageOptions _options;
    private readonly ILogger<S3MediaStorage> _logger;

    public S3MediaStorage(IAmazonS3 s3Client, IOptions<S3MediaStorageOptions> options, ILogger<S3MediaStorage> logger)
    {
        _s3Client = s3Client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MediaUploadResult> UploadAsync(Stream content, string bucketPath, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = $"assets/{bucketPath}/{fileName}";
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request, ct);
            var publicUrl = $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";

            return new MediaUploadResult(true, Key: key, PublicUrl: publicUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload {Key} to S3", key);
            return new MediaUploadResult(false, Error: ex.Message);
        }
    }

    public async Task<bool> DeleteAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        var keyVersions = keys.Select(k => new KeyVersion { Key = k }).ToList();
        if (keyVersions.Count == 0) return true;

        try
        {
            var response = await _s3Client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = _options.BucketName,
                Objects = keyVersions
            }, ct);

            return response.DeletedObjects.Count == keyVersions.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete {Count} objects from S3", keyVersions.Count);
            return false;
        }
    }
}
