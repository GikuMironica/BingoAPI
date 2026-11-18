using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Hopaut.BuildingBlocks.UnitTests.Application;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task GivenAFailedResult_WhenHandled_ThenItIsReturnedUnchangedAndItsErrorCodeIsLogged()
    {
        // Arrange
        var logger = new RecordingLogger<LoggingBehavior<CloseVenue, Result>>();
        var behavior = new LoggingBehavior<CloseVenue, Result>(logger);
        var closed = Result.Failure(Error.Conflict("venues.venue.closed", "Venue 7 is closed."));

        // Act
        var result = await behavior.Handle(new CloseVenue(7), (_, _) => ValueTask.FromResult(closed), TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeSameAs(closed);
            logger.Messages.Should().Equal("Handling CloseVenue", "Handled CloseVenue with error venues.venue.closed (Conflict)");
        }
    }

    public sealed record CloseVenue(int VenueId) : ICommand<Result>;

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }
}
