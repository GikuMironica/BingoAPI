using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class TextValueObjectTests
{
    [Theory]
    [InlineData("  Alice@Hopaut.App ", "alice@hopaut.app")]
    [InlineData("first.last+tag@mail.example.org", "first.last+tag@mail.example.org")]
    public void GivenAWellFormedAddress_WhenAnEmailAddressIsCreated_ThenItIsTrimmedAndLowerCased(string input, string expected)
    {
        // Act
        var result = EmailAddress.Create(input);

        // Assert
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "shared.email_address.required")]
    [InlineData("   ", "shared.email_address.required")]
    [InlineData("alice", "shared.email_address.invalid")]
    [InlineData("@hopaut.app", "shared.email_address.invalid")]
    [InlineData("alice@hopaut", "shared.email_address.invalid")]
    [InlineData("alice@@hopaut.app", "shared.email_address.invalid")]
    [InlineData("alice@hopaut..app", "shared.email_address.invalid")]
    [InlineData("alice@.hopaut.app", "shared.email_address.invalid")]
    [InlineData("ali ce@hopaut.app", "shared.email_address.invalid")]
    [InlineData("Alice <alice@hopaut.app>", "shared.email_address.invalid")]
    public void GivenAMissingOrMalformedAddress_WhenAnEmailAddressIsCreated_ThenItFails(string? input, string code)
    {
        // Act
        var result = EmailAddress.Create(input);

        // Assert
        result.Error.Code.Should().Be(code);
    }

    [Fact]
    public void GivenAnAddressLongerThan254Characters_WhenAnEmailAddressIsCreated_ThenItIsTooLong()
    {
        // Arrange
        var address = new string('a', 64) + "@" + new string('b', 186) + ".app";

        // Act
        var result = EmailAddress.Create(address);

        // Assert
        result.Error.Code.Should().Be("shared.email_address.too_long");
    }

    [Fact]
    public void GivenALocalPartLongerThan64Characters_WhenAnEmailAddressIsCreated_ThenItIsInvalid()
    {
        // Act
        var result = EmailAddress.Create(new string('a', 65) + "@hopaut.app");

        // Assert
        result.Error.Code.Should().Be("shared.email_address.invalid");
    }

    [Theory]
    [InlineData("  House party  ", "House party")]
    [InlineData("x", "x")]
    public void GivenATitleWithinThirtyCharacters_WhenAPostTitleIsCreated_ThenItIsTrimmed(string input, string expected)
    {
        // Act
        var result = PostTitle.Create(input);

        // Assert
        result.Value.Value.Should().Be(expected);
    }

    [Fact]
    public void GivenTitlesAtAndBeyondTheLimit_WhenPostTitlesAreCreated_ThenOnlyTheLongerOneFails()
    {
        // Act
        var atLimit = PostTitle.Create(new string('t', PostTitle.MaxLength));
        var beyond = PostTitle.Create(new string('t', PostTitle.MaxLength + 1));

        // Assert
        using (new AssertionScope())
        {
            atLimit.IsSuccess.Should().BeTrue();
            beyond.Error.Code.Should().Be("shared.post_title.too_long");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GivenABlankTitle_WhenAPostTitleIsCreated_ThenItIsRequired(string? input)
    {
        // Act
        var result = PostTitle.Create(input);

        // Assert
        result.Error.Code.Should().Be("shared.post_title.required");
    }

    [Theory]
    [InlineData(null, "shared.post_description.too_short")]
    [InlineData("   nine 9   ", "shared.post_description.too_short")]
    public void GivenADescriptionShorterThanTenCharactersAfterTrimming_WhenCreated_ThenItIsTooShort(string? input, string code)
    {
        // Act
        var result = PostDescription.Create(input);

        // Assert
        result.Error.Code.Should().Be(code);
    }

    [Fact]
    public void GivenDescriptionsAtTheLimits_WhenCreated_ThenOnlyTheLongerOneFails()
    {
        // Act
        var shortest = PostDescription.Create("  " + new string('d', PostDescription.MinLength) + "  ");
        var longest = PostDescription.Create(new string('d', PostDescription.MaxLength));
        var beyond = PostDescription.Create(new string('d', PostDescription.MaxLength + 1));

        // Assert
        using (new AssertionScope())
        {
            shortest.Value.Value.Should().HaveLength(PostDescription.MinLength);
            longest.IsSuccess.Should().BeTrue();
            beyond.Error.Code.Should().Be("shared.post_description.too_long");
        }
    }

    [Theory]
    [InlineData("assets/posts/0193a1b2-c3d4.webp")]
    [InlineData("assets/profile_pictures/user-1/avatar.v2.jpg")]
    [InlineData("a")]
    public void GivenASafeKey_WhenAMediaKeyIsCreated_ThenItIsKept(string input)
    {
        // Act
        var result = MediaKey.Create(input);

        // Assert
        result.Value.Value.Should().Be(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/assets/posts/a.webp")]
    [InlineData("assets/posts/")]
    [InlineData("assets//a.webp")]
    [InlineData("assets/../secrets/a.webp")]
    [InlineData("assets/./a.webp")]
    [InlineData("assets/posts/a b.webp")]
    [InlineData("assets\\posts\\a.webp")]
    [InlineData("assets/posts/ä.webp")]
    public void GivenAnUnsafeOrEmptyKey_WhenAMediaKeyIsCreated_ThenItFails(string? input)
    {
        // Act
        var result = MediaKey.Create(input);

        // Assert
        result.Error.Code.Should().Be("shared.media_key.invalid");
    }

    [Fact]
    public void GivenAKeyLongerThan1024Characters_WhenAMediaKeyIsCreated_ThenItFails()
    {
        // Act
        var result = MediaKey.Create(new string('k', MediaKey.MaxLength + 1));

        // Assert
        result.Error.Code.Should().Be("shared.media_key.invalid");
    }
}
