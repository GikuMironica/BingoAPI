using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Application.Caching;
using Hopaut.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Hopaut.BuildingBlocks.UnitTests.Application;

public sealed class CachingBehaviorTests
{
    [Fact]
    public async Task GivenACachedResult_WhenHandled_ThenItIsReturnedWithoutCallingTheHandler()
    {
        // Arrange
        var cache = Substitute.For<ICacheService>();
        cache.GetAsync<Result<int>>("venue:7", Arg.Any<CancellationToken>()).Returns(Result.Success(42));
        var behavior = new CachingBehavior<GetVenueCapacity, Result<int>>(cache, NullLogger<CachingBehavior<GetVenueCapacity, Result<int>>>.Instance);
        var handlerCalls = 0;

        // Act
        var result = await behavior.Handle(
            new GetVenueCapacity(7),
            (_, _) => { handlerCalls++; return ValueTask.FromResult<Result<int>>(1); },
            TestContext.Current.CancellationToken);

        // Assert
        using (new AssertionScope())
        {
            handlerCalls.Should().Be(0);
            result.Value.Should().Be(42);
        }
    }

    [Fact]
    public async Task GivenACacheMissAndASuccess_WhenHandled_ThenTheResultIsCachedWithTheQueryOptions()
    {
        // Arrange
        var cache = Substitute.For<ICacheService>();
        var behavior = new CachingBehavior<GetVenueCapacity, Result<int>>(cache, NullLogger<CachingBehavior<GetVenueCapacity, Result<int>>>.Instance);

        // Act
        await behavior.Handle(new GetVenueCapacity(7), (_, _) => ValueTask.FromResult<Result<int>>(42), TestContext.Current.CancellationToken);

        // Assert
        await cache.Received(1).SetAsync(
            "venue:7",
            Arg.Is<Result<int>>(r => r.IsSuccess && r.Value == 42),
            TimeSpan.FromMinutes(2),
            Arg.Is<string[]?>(tags => tags != null && tags.SequenceEqual(new[] { "venues" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivenACacheMissAndAFailure_WhenHandled_ThenNothingIsCached()
    {
        // Arrange
        var cache = Substitute.For<ICacheService>();
        var behavior = new CachingBehavior<GetVenueCapacity, Result<int>>(cache, NullLogger<CachingBehavior<GetVenueCapacity, Result<int>>>.Instance);
        var notFound = Error.NotFound("venues.venue.not_found", "Venue 7 was not found.");

        // Act
        await behavior.Handle(new GetVenueCapacity(7), (_, _) => ValueTask.FromResult<Result<int>>(notFound), TestContext.Current.CancellationToken);

        // Assert
        await cache.DidNotReceiveWithAnyArgs().SetAsync<Result<int>>(default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    public sealed record GetVenueCapacity(int VenueId) : IQuery<Result<int>>, ICachedQuery
    {
        public string CacheKey => $"venue:{VenueId}";

        public TimeSpan? Expiry => TimeSpan.FromMinutes(2);

        public string[]? Tags => ["venues"];
    }
}
