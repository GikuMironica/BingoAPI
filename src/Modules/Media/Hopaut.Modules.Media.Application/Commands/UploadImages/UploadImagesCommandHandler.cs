using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Media.Application.Commands.UploadImages;

public sealed class UploadImagesCommandHandler(IImageProcessor imageProcessor, IMediaStorage mediaStorage, TimeProvider timeProvider)
    : ICommandHandler<UploadImagesCommand, Result<IReadOnlyList<string>>>
{
    public async ValueTask<Result<IReadOnlyList<string>>> Handle(UploadImagesCommand request, CancellationToken cancellationToken)
    {
        var keys = new List<string>();

        foreach (var image in request.Images)
        {
            var processResult = await imageProcessor.ProcessAsync(image.Content, ct: cancellationToken);
            if (!processResult.Success)
                return MediaUploadErrors.ProcessingFailed(processResult.Error);

            var fileName = $"{Guid.CreateVersion7(timeProvider.GetUtcNow())}.webp";
            var uploadResult = await mediaStorage.UploadAsync(
                processResult.Output!,
                request.BucketPath,
                fileName,
                processResult.ContentType,
                cancellationToken);

            if (!uploadResult.Success)
                return MediaUploadErrors.StorageFailed(uploadResult.Error);

            keys.Add(uploadResult.Key!);

            // Dispose processed stream
            if (processResult.Output is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else
                processResult.Output?.Dispose();
        }

        return Result.Success<IReadOnlyList<string>>(keys);
    }
}
