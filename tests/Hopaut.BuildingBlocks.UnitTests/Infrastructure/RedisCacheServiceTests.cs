using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure.Caching;
using Hopaut.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Hopaut.BuildingBlocks.UnitTests.Infrastructure;

/// <summary>CachingBehavior stores successful <see cref="Result"/> responses; they must survive the JSON round trip.</summary>
public sealed class RedisCacheServiceTests
{
    private readonly RedisCacheService _cache = new(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));

    [Fact]
    public async Task GivenASuccessfulValueResult_WhenCachedAndRead_ThenTheValueRoundTrips()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        Result<VenueDto> original = new VenueDto(7, "Hopaut Hall", ["outdoor", "music"]);

        // Act
        await _cache.SetAsync("venue:7", original, ct: ct);
        var cached = await _cache.GetAsync<Result<VenueDto>>("venue:7", ct);

        // Assert
        using (new AssertionScope())
        {
            cached!.IsSuccess.Should().BeTrue();
            cached.Value.Should().BeEquivalentTo(original.Value);
        }
    }

    [Fact]
    public async Task GivenASuccessfulResultWithoutValue_WhenCachedAndRead_ThenItIsASuccess()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        await _cache.SetAsync("venue:open", Result.Success(), ct: ct);
        var cached = await _cache.GetAsync<Result>("venue:open", ct);

        // Assert
        cached!.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GivenAFailedResult_WhenCached_ThenItIsRejected()
    {
        // Arrange
        Result<VenueDto> failed = Error.NotFound("venues.venue.not_found", "Venue 7 was not found.");

        // Act
        var act = () => _cache.SetAsync("venue:7", failed, ct: TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*venues.venue.not_found*");
    }

    public sealed record VenueDto(int Id, string Name, IReadOnlyList<string> Tags);
}
