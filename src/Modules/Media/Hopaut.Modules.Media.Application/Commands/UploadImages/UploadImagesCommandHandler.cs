using MediatR;

namespace Hopaut.Modules.Media.Application.Commands.UploadImages;

public sealed class UploadImagesCommandHandler : IRequestHandler<UploadImagesCommand, UploadImagesResult>
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IMediaStorage _mediaStorage;

    public UploadImagesCommandHandler(IImageProcessor imageProcessor, IMediaStorage mediaStorage)
    {
        _imageProcessor = imageProcessor;
        _mediaStorage = mediaStorage;
    }

    public async Task<UploadImagesResult> Handle(UploadImagesCommand request, CancellationToken cancellationToken)
    {
        var keys = new List<string>();

        foreach (var image in request.Images)
        {
            var processResult = await _imageProcessor.ProcessAsync(image.Content, ct: cancellationToken);
            if (!processResult.Success)
                return new UploadImagesResult(false, [], processResult.Error);

            var fileName = $"{Guid.NewGuid()}.webp";
            var uploadResult = await _mediaStorage.UploadAsync(
                processResult.Output!,
                request.BucketPath,
                fileName,
                processResult.ContentType,
                cancellationToken);

            if (!uploadResult.Success)
                return new UploadImagesResult(false, [], uploadResult.Error);

            keys.Add(uploadResult.Key!);

            // Dispose processed stream
            if (processResult.Output is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                processResult.Output?.Dispose();
        }

        return new UploadImagesResult(true, keys);
    }
}
