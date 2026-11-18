using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.Modules.Media.Application.Commands.UploadImages;

/// <summary>Processes and stores the images; the result holds the storage keys in upload order.</summary>
public sealed record UploadImagesCommand(
    IReadOnlyList<ImageInput> Images,
    string BucketPath,
    string UploadedByUserId) : ICommand<Result<IReadOnlyList<string>>>, IMediaCommand;

public sealed record ImageInput(Stream Content, string OriginalFileName, string ContentType);
