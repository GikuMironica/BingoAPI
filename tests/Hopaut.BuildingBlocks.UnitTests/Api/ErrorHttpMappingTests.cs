using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Api.Http;
using Hopaut.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace Hopaut.BuildingBlocks.UnitTests.Api;

public sealed class ErrorHttpMappingTests
{
    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Unauthorized, StatusCodes.Status401Unauthorized)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Rule, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    public void GivenAnErrorType_WhenMapped_ThenItYieldsTheAgreedStatus(ErrorType type, int expected)
    {
        // Act
        var status = type.ToStatusCode();

        // Assert
        status.Should().Be(expected);
    }

    [Fact]
    public void GivenEveryErrorType_WhenMapped_ThenNoneIsLeftUnmapped()
    {
        // Act
        var act = () => Enum.GetValues<ErrorType>().Select(t => t.ToStatusCode()).ToList();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GivenADomainError_WhenTurnedIntoAProblem_ThenItCarriesStatusDetailAndErrorCode()
    {
        // Arrange
        var error = Error.Conflict("attendance.participation.duplicate", "Already requested.");

        // Act
        var problem = error.ToProblem();

        // Assert
        using (new AssertionScope())
        {
            problem.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            problem.ProblemDetails.Detail.Should().Be("Already requested.");
            problem.ProblemDetails.Extensions.Should().Contain(ErrorHttpMapping.ErrorCodeExtension, "attendance.participation.duplicate");
            problem.ContentType.Should().Be("application/problem+json");
        }
    }

    [Fact]
    public void GivenAValidationError_WhenTurnedIntoAProblem_ThenTheFieldErrorsAreIncluded()
    {
        // Arrange
        var error = new ValidationError(new Dictionary<string, string[]> { ["radiusMeters"] = ["Must be at most 50000."] });

        // Act
        var problem = error.ToProblem();

        // Assert
        using (new AssertionScope())
        {
            problem.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            problem.ProblemDetails.Should().BeOfType<Microsoft.AspNetCore.Http.HttpValidationProblemDetails>()
                .Which.Errors.Should().ContainKey("radiusMeters");
        }
    }

    [Fact]
    public void GivenASuccessfulResult_WhenTurnedIntoAProblem_ThenItThrows()
    {
        // Act
        var act = () => Result.Success().ToProblem();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
