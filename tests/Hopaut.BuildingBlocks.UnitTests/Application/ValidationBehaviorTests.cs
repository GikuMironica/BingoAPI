using AwesomeAssertions.Execution;
using FluentValidation;
using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;

namespace Hopaut.BuildingBlocks.UnitTests.Application;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task GivenAnInvalidCommand_WhenHandled_ThenItShortCircuitsWithACamelCaseValidationError()
    {
        // Arrange
        var behavior = new ValidationBehavior<RegisterVenue, Result<int>>([new RegisterVenueValidator()]);
        var handlerCalls = 0;

        // Act
        var result = await behavior.Handle(
            new RegisterVenue(string.Empty, new VenueAddress(string.Empty)),
            (_, _) => { handlerCalls++; return ValueTask.FromResult<Result<int>>(7); },
            TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            handlerCalls.Should().Be(0);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ValidationError>()
                .Which.Errors.Keys.Should().BeEquivalentTo(["name", "address.city"]);
        }
    }

    [Fact]
    public async Task GivenAValidCommand_WhenHandled_ThenTheHandlerResultIsReturned()
    {
        // Arrange
        var behavior = new ValidationBehavior<RegisterVenue, Result<int>>([new RegisterVenueValidator()]);

        // Act
        var result = await behavior.Handle(
            new RegisterVenue("Hopaut Hall", new VenueAddress("Berlin")),
            (_, _) => ValueTask.FromResult<Result<int>>(7),
            TestContext.Current.CancellationToken);

        // Assert
        result.Value.Should().Be(7);
    }

    [Fact]
    public async Task GivenNoValidator_WhenHandled_ThenTheHandlerRuns()
    {
        // Arrange
        var behavior = new ValidationBehavior<CloseVenue, Result>([]);
        var handlerCalls = 0;

        // Act
        var result = await behavior.Handle(
            new CloseVenue(string.Empty),
            (_, _) => { handlerCalls++; return ValueTask.FromResult(Result.Success()); },
            TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            handlerCalls.Should().Be(1);
            result.IsSuccess.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GivenAnInvalidCommandWithoutValue_WhenHandled_ThenAFailedResultIsReturned()
    {
        // Arrange
        var behavior = new ValidationBehavior<CloseVenue, Result>([new CloseVenueValidator()]);

        // Act
        var result = await behavior.Handle(
            new CloseVenue(string.Empty),
            (_, _) => ValueTask.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        // Assert
        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("reason");
    }

    public sealed record VenueAddress(string City);

    public sealed record RegisterVenue(string Name, VenueAddress Address) : ICommand<Result<int>>;

    public sealed record CloseVenue(string Reason) : ICommand<Result>;

    private sealed class RegisterVenueValidator : AbstractValidator<RegisterVenue>
    {
        public RegisterVenueValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Address.City).NotEmpty();
        }
    }

    private sealed class CloseVenueValidator : AbstractValidator<CloseVenue>
    {
        public CloseVenueValidator() => RuleFor(x => x.Reason).NotEmpty();
    }
}
