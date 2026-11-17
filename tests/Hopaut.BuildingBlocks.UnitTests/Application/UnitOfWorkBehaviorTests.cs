using Hopaut.BuildingBlocks.Application.Behaviors;
using Hopaut.SharedKernel;
using Mediator;
using NSubstitute;

namespace Hopaut.BuildingBlocks.UnitTests.Application;

public sealed class UnitOfWorkBehaviorTests
{
    private static readonly Error Rejected = Error.Conflict("venues.venue.closed", "Venue 7 is closed.");

    [Fact]
    public async Task GivenASuccessfulResult_WhenHandled_ThenTheUnitOfWorkSavesExactlyOnce()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new SampleUnitOfWorkBehavior<RenameVenue, Result>(unitOfWork);

        // Act
        await behavior.Handle(new RenameVenue("Hall"), (_, _) => ValueTask.FromResult(Result.Success()), TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenAFailedResult_WhenHandled_ThenTheUnitOfWorkSavesNothing()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new SampleUnitOfWorkBehavior<RenameVenue, Result>(unitOfWork);

        // Act
        await behavior.Handle(new RenameVenue("Hall"), (_, _) => ValueTask.FromResult(Result.Failure(Rejected)), TestContext.Current.CancellationToken);

        // Assert
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenAThrowingHandler_WhenHandled_ThenTheExceptionPropagatesAndNothingIsSaved()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new SampleUnitOfWorkBehavior<RenameVenue, Result>(unitOfWork);

        // Act
        var act = async () => await behavior.Handle(
            new RenameVenue("Hall"),
            (_, _) => throw new InvalidOperationException("handler failed"),
            TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenASuccessfulValueResult_WhenHandled_ThenTheValueIsReturnedAfterTheSave()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var behavior = new SampleUnitOfWorkBehavior<OpenVenue, Result<int>>(unitOfWork);

        // Act
        var result = await behavior.Handle(new OpenVenue("Hall"), (_, _) => ValueTask.FromResult<Result<int>>(7), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Should().Be(7);
    }

    public sealed record RenameVenue(string Name) : ICommand<Result>;

    public sealed record OpenVenue(string Name) : ICommand<Result<int>>;

    private sealed class SampleUnitOfWorkBehavior<TMessage, TResponse>(IUnitOfWork unitOfWork)
        : UnitOfWorkBehavior<TMessage, TResponse>(unitOfWork)
        where TMessage : IBaseCommand
        where TResponse : IResultBase;
}
