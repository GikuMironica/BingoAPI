using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.Infrastructure;

public sealed class StronglyTypedIdConverterTests
{
    [Fact]
    public void GivenAnIntBackedId_WhenConvertedToTheColumnAndBack_ThenTheValueRoundTrips()
    {
        // Arrange
        var converter = new StronglyTypedIdConverter<PostId, int>();

        // Act
        var column = converter.ConvertToProvider(new PostId(42));
        var restored = converter.ConvertFromProvider(column);

        // Assert
        using (new AssertionScope())
        {
            column.Should().Be(42);
            restored.Should().Be(new PostId(42));
        }
    }

    [Fact]
    public void GivenAStringBackedId_WhenConvertedToTheColumnAndBack_ThenTheValueRoundTrips()
    {
        // Arrange
        var converter = new StronglyTypedIdConverter<UserId, string>();

        // Act
        var column = converter.ConvertToProvider(new UserId("user-1"));
        var restored = converter.ConvertFromProvider(column);

        // Assert
        using (new AssertionScope())
        {
            column.Should().Be("user-1");
            restored.Should().Be(new UserId("user-1"));
        }
    }

    [Fact]
    public void GivenAStoredKeyThatParsingWouldReject_WhenMaterialized_ThenItIsWrappedAsIs()
    {
        // Arrange
        var converter = new StronglyTypedIdConverter<PostId, int>();

        // Act
        var restored = converter.ConvertFromProvider(0);

        // Assert
        restored.Should().Be(new PostId(0));
    }
}
