using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class DomainEventBaseTests
{
    private static readonly DateTimeOffset OccurredOn = new(2026, 1, 9, 12, 30, 0, TimeSpan.FromHours(2));

    [Fact]
    public void GivenAnOccurrenceTime_WhenAnEventIsCreated_ThenItsIdIsAVersion7GuidStampedWithThatTime()
    {
        // Act
        var domainEvent = new SampleEvent(OccurredOn);

        // Assert
        using (new AssertionScope())
        {
            domainEvent.EventId.Version.Should().Be(7);
            GuidTimestamp.UnixMilliseconds(domainEvent.EventId).Should().Be(OccurredOn.ToUnixTimeMilliseconds());
        }
    }

    [Fact]
    public void GivenAnOccurrenceTimeWithAnOffset_WhenAnEventIsCreated_ThenTheTimeIsNormalizedToUtc()
    {
        // Act
        var domainEvent = new SampleEvent(OccurredOn);

        // Assert
        domainEvent.OccurredOnUtc.Should().BeExactly(OccurredOn.ToUniversalTime());
    }

    [Fact]
    public void GivenTheSameOccurrenceTime_WhenTwoEventsAreCreated_ThenTheirIdsDiffer()
    {
        // Act
        var first = new SampleEvent(OccurredOn);
        var second = new SampleEvent(OccurredOn);

        // Assert
        first.EventId.Should().NotBe(second.EventId);
    }

    private sealed record SampleEvent(DateTimeOffset OccurredOnUtc) : DomainEventBase(OccurredOnUtc);
}

/// <summary>Reads the 48-bit Unix-millisecond timestamp of a version-7 Guid (RFC 9562 §5.7).</summary>
internal static class GuidTimestamp
{
    public static long UnixMilliseconds(Guid guid)
    {
        var bytes = guid.ToByteArray(bigEndian: true);
        return bytes.Take(6).Aggregate(0L, (timestamp, b) => (timestamp << 8) | b);
    }
}
