using AwesomeAssertions.Execution;
using Hopaut.Modules.Media.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hopaut.Modules.Media.UnitTests.Infrastructure;

/// <summary>Pins the decode → resize → WebP pipeline so image-library upgrades (e.g. blocker B-001) are verified.</summary>
public sealed class ImageSharpProcessorTests
{
    [Fact]
    public async Task GivenAPngLargerThanTheBounds_WhenProcessed_ThenItIsShrunkKeepingTheAspectRatioAndEncodedAsWebp()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var input = await PngAsync(3000, 2000, ct);

        // Act
        var result = await new ImageSharpProcessor().ProcessAsync(input, maxWidth: 1920, maxHeight: 1080, ct);

        // Assert
        await using var output = result.Output;
        result.Success.Should().BeTrue(result.Error);
        var format = await Image.DetectFormatAsync(output!, ct);
        output!.Position = 0;
        var info = await Image.IdentifyAsync(output, ct);
        using (new AssertionScope())
        {
            format.Should().Be(WebpFormat.Instance);
            result.ContentType.Should().Be("image/webp");
            info.Width.Should().Be(1620);
            info.Height.Should().Be(1080);
        }
    }

    [Fact]
    public async Task GivenBytesThatAreNotAnImage_WhenProcessed_ThenAFailedResultIsReturned()
    {
        // Arrange
        await using var input = new MemoryStream("not an image"u8.ToArray());

        // Act
        var result = await new ImageSharpProcessor().ProcessAsync(input, ct: TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Success.Should().BeFalse();
            result.Output.Should().BeNull();
            result.Error.Should().NotBeNullOrWhiteSpace();
        }
    }

    private static async Task<MemoryStream> PngAsync(int width, int height, CancellationToken ct)
    {
        using var image = new Image<Rgba32>(width, height, new Rgba32(100, 149, 237));
        var stream = new MemoryStream();
        await image.SaveAsPngAsync(stream, ct);
        stream.Position = 0;
        return stream;
    }
}
