using System.Globalization;
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
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("+5")]
    [InlineData(" 42")]
    [InlineData("4 2")]
    [InlineData("2147483648")]
    public void GivenANonPositiveSignedPaddedOrOverflowingNumber_WhenParsedAsPostId_ThenParsingFails(string input)
    {
        // Act
        var parsed = PostId.TryParse(input, CultureInfo.InvariantCulture, out _);

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

    [Theory]
    [InlineData("0196a5c8-3f1e-7c2a-9b4d-2f6e8a1c3d5e", true)]
    [InlineData("00000000-0000-0000-0000-000000000000", false)]
    [InlineData("not-a-guid", false)]
    [InlineData(null, false)]
    public void GivenAGuidString_WhenParsedAsAGuidBackedId_ThenOnlyNonEmptyGuidsAreAccepted(string? input, bool expected)
    {
        // Act
        var parsed = SampleGuidId.TryParse(input, CultureInfo.InvariantCulture, out _);

        // Assert
        parsed.Should().Be(expected);
    }

    [Fact]
    public void GivenARouteValue_WhenParsedThroughIParsable_ThenTheIdIsCreated()
    {
        // Act
        var id = ParseThroughInterface<RatingId>("7");

        // Assert
        id.Should().Be(new RatingId(7));
    }

    [Fact]
    public void GivenAnInvalidValue_WhenParsed_ThenAFormatExceptionIsThrown()
    {
        // Act
        var act = () => PostId.Parse("0", CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void GivenAnInvalidValue_WhenCreated_ThenCreationFails()
    {
        // Act
        var created = StronglyTypedId.TryCreate<PostId, int>(0, out var id);

        // Assert
        using (new AssertionScope())
        {
            created.Should().BeFalse();
            id.Should().Be(default(PostId));
        }
    }

    private static T ParseThroughInterface<T>(string s) where T : IParsable<T> => T.Parse(s, CultureInfo.InvariantCulture);
}
