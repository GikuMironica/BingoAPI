using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Domain;

namespace Hopaut.Modules.Posts.UnitTests.Domain;

public sealed class TagNameTests
{
    [Fact]
    public void Create_WithPaddingAndCapitals_TrimsAndLowerCases()
    {
        // Act
        var name = TagName.Create("  Rooftop PARTY ");

        // Assert
        name.Value.Value.Should().Be("rooftop party");
    }

    [Theory]
    [InlineData(null, "posts.tag.name_required")]
    [InlineData("   ", "posts.tag.name_required")]
    [InlineData("abcdefghijklmnopqrstu", "posts.tag.name_too_long")]
    public void Create_WithAnInvalidName_FailsValidation(string? value, string code)
    {
        // Act
        var name = TagName.Create(value);

        // Assert
        using (new AssertionScope())
        {
            name.Error.Code.Should().Be(code);
            name.Error.Type.Should().Be(Hopaut.SharedKernel.ErrorType.Validation);
        }
    }

    [Fact]
    public void Create_WithExactlyTwentyCharacters_Succeeds()
    {
        // Act
        var name = TagName.Create(new string('a', TagName.MaxLength));

        // Assert
        name.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CreateMany_WithDuplicatesAfterNormalisation_KeepsEachNameOnceInOrder()
    {
        // Act
        var names = TagName.CreateMany(["Jazz", " jazz", "Rock", "JAZZ "]);

        // Assert
        names.Value.Select(name => name.Value).Should().Equal("jazz", "rock");
    }

    [Fact]
    public void CreateMany_WithoutAList_IsEmpty()
    {
        // Act
        var names = TagName.CreateMany(null);

        // Assert
        names.Value.Should().BeEmpty();
    }

    [Fact]
    public void CreateMany_WithMoreThanTwentyDistinctNames_FailsWithTooManyTags()
    {
        // Act
        var names = TagName.CreateMany(Enumerable.Range(1, Post.MaxTags + 1).Select(i => $"tag{i}"));

        // Assert
        names.Error.Code.Should().Be("posts.post.too_many_tags");
    }

    [Fact]
    public void CreateMany_WithOneInvalidName_FailsWithThatNamesError()
    {
        // Act
        var names = TagName.CreateMany(["jazz", ""]);

        // Assert
        names.Error.Code.Should().Be("posts.tag.name_required");
    }
}
