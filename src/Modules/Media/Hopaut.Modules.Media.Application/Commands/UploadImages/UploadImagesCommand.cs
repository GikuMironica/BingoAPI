using MediatR;

namespace Hopaut.Modules.Media.Application.Commands.UploadImages;

public sealed record UploadImagesCommand(
    IReadOnlyList<ImageInput> Images,
    string BucketPath,
    string UploadedByUserId) : IRequest<UploadImagesResult>;

public sealed record ImageInput(Stream Content, string OriginalFileName, string ContentType);

public sealed record UploadImagesResult(bool Success, IReadOnlyList<string> ImageKeys, string? Error = null);
