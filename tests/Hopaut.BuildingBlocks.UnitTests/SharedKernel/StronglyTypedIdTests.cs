using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class StronglyTypedIdTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("1", 1)]
    public void GivenANumericString_WhenParsedAsPostId_ThenTheValueIsKept(string input, int expected)
    {
        // Act
        var parsed = PostId.TryParse(input, out var id);

        // Assert
        using (new AssertionScope())
        {
            parsed.Should().BeTrue();
            id.Value.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(null)]
    public void GivenANonNumericString_WhenParsedAsPostId_ThenParsingFails(string? input)
    {
        // Act
        var parsed = PostId.TryParse(input, out _);

        // Assert
        parsed.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GivenABlankString_WhenParsedAsUserId_ThenParsingFails(string? input)
    {
        // Act
        var parsed = UserId.TryParse(input, out _);

        // Assert
        parsed.Should().BeFalse();
    }

    [Fact]
    public void GivenAUserIdString_WhenParsed_ThenTheValueIsKept()
    {
        // Act
        var parsed = UserId.TryParse("user-123", out var id);

        // Assert
        using (new AssertionScope())
        {
            parsed.Should().BeTrue();
            id.Value.Should().Be("user-123");
        }
    }
}
