using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class ResultTests
{
    private static readonly Error NotFound = Error.NotFound("posts.post.not_found", "Post 7 was not found.");

    [Fact]
    public void GivenASuccess_WhenInspected_ThenItCarriesNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Error.Should().Be(Error.None);
        }
    }

    [Fact]
    public void GivenAFailure_WhenInspected_ThenItCarriesTheError()
    {
        // Act
        var result = Result.Failure(NotFound);

        // Assert
        using (new AssertionScope())
        {
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(NotFound);
        }
    }

    [Fact]
    public void GivenErrorNone_WhenUsedAsAFailure_ThenItIsRejected()
    {
        // Act
        var act = () => Result.Failure(Error.None);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenASuccessfulValueResult_WhenTheValueIsRead_ThenItIsReturned()
    {
        // Act
        Result<int> result = 42;

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
        }
    }

    [Fact]
    public void GivenAFailedValueResult_WhenTheValueIsRead_ThenItThrows()
    {
        // Arrange
        Result<int> result = NotFound;

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*posts.post.not_found*");
    }

    [Fact]
    public void GivenFieldErrors_WhenAValidationErrorIsCreated_ThenItUsesTheValidationTypeAndCode()
    {
        // Arrange
        var fields = new Dictionary<string, string[]> { ["title"] = ["Title is required."] };

        // Act
        var error = new ValidationError(fields);

        // Assert
        using (new AssertionScope())
        {
            error.Type.Should().Be(ErrorType.Validation);
            error.Code.Should().Be(ValidationError.DefaultCode);
            error.Errors.Should().ContainKey("title");
        }
    }
}
