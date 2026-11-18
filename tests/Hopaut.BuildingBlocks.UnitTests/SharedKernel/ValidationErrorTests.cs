using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class ValidationErrorTests
{
    [Fact]
    public void GivenSomeFailedResults_WhenCollected_ThenEachFieldListsItsMessages()
    {
        // Act
        var error = ValidationError.FromFailures(
            ("title", PostTitle.Create(null)),
            ("slots", Capacity.Create(5)),
            ("title", PostTitle.Create(new string('t', 31))),
            ("rate", RatingValue.Create(9)));

        // Assert
        using (new AssertionScope())
        {
            error.Should().NotBeNull();
            error!.Code.Should().Be(ValidationError.DefaultCode);
            error.Errors.Keys.Should().BeEquivalentTo("title", "rate");
            error.Errors["title"].Should().Equal(PostTextErrors.TitleRequired.Description, PostTextErrors.TitleTooLong.Description);
        }
    }

    [Fact]
    public void GivenOnlySuccessesAndAbsentOptionalInputs_WhenCollected_ThenThereIsNoError()
    {
        // Act
        var error = ValidationError.FromFailures(("slots", Capacity.Create(5)), ("deposit", null));

        // Assert
        error.Should().BeNull();
    }
}
