using Hopaut.Modules.Media.Application;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Hopaut.Modules.Media.Infrastructure;

public sealed class ImageSharpProcessor : IImageProcessor
{
    public async Task<ImageProcessResult> ProcessAsync(Stream input, int maxWidth = 1920, int maxHeight = 1080, CancellationToken ct = default)
    {
        try
        {
            using var image = await Image.LoadAsync(input, ct);

            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));

            var output = new MemoryStream();
            await image.SaveAsync(output, new WebpEncoder { Quality = 80 }, ct);
            output.Position = 0;

            return new ImageProcessResult(true, Output: output);
        }
        catch (Exception ex)
        {
            return new ImageProcessResult(false, Error: ex.Message);
        }
    }
}
