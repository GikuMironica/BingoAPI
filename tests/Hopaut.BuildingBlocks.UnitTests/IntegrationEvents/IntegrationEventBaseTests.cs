using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.UnitTests.SharedKernel;
using Hopaut.IntegrationEvents;
using Hopaut.SharedKernel;
using Hopaut.SharedKernel.Serialization;

namespace Hopaut.BuildingBlocks.UnitTests.IntegrationEvents;

public sealed class IntegrationEventBaseTests
{
    private static readonly DateTimeOffset OccurredOn = new(2026, 1, 9, 12, 30, 0, TimeSpan.Zero);

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new StronglyTypedIdJsonConverterFactory() },
    };

    public sealed record SampleIntegrationEvent(DateTimeOffset OccurredOnUtc, PostId PostId) : IntegrationEventBase(OccurredOnUtc);

    [Fact]
    public void GivenAnOccurrenceTime_WhenAnEventIsCreated_ThenItsIdIsAVersion7GuidStampedWithThatTime()
    {
        // Act
        var integrationEvent = new SampleIntegrationEvent(OccurredOn, new PostId(42));

        // Assert
        using (new AssertionScope())
        {
            integrationEvent.EventId.Version.Should().Be(7);
            GuidTimestamp.UnixMilliseconds(integrationEvent.EventId).Should().Be(OccurredOn.ToUnixTimeMilliseconds());
        }
    }

    [Fact]
    public void GivenASerializedEvent_WhenDeserialized_ThenItKeepsTheEventIdConsumersDeduplicateOn()
    {
        // Arrange
        var original = new SampleIntegrationEvent(OccurredOn, new PostId(42));
        var json = JsonSerializer.Serialize(original, Options);

        // Act
        var restored = JsonSerializer.Deserialize<SampleIntegrationEvent>(json, Options);

        // Assert
        using (new AssertionScope())
        {
            restored.Should().NotBeNull();
            restored!.EventId.Should().Be(original.EventId);
            restored.OccurredOnUtc.Should().BeExactly(OccurredOn);
            restored.PostId.Should().Be(original.PostId);
        }
    }
}
