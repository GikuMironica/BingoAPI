using System.Text.Json;
using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;
using Hopaut.SharedKernel.Serialization;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class StronglyTypedIdJsonConverterTests
{
    private const string MediaGuid = "0196a5c8-3f1e-7c2a-9b4d-2f6e8a1c3d5e";

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new StronglyTypedIdJsonConverterFactory() },
    };

    public sealed record Payload(PostId PostId, UserId UserId, SampleGuidId MediaId, PostId? ParentId);

    [Fact]
    public void GivenAPayloadWithIds_WhenSerialized_ThenEachIdIsWrittenAsItsPrimitiveValue()
    {
        // Arrange
        var payload = new Payload(new PostId(42), new UserId("user-1"), new SampleGuidId(Guid.Parse(MediaGuid)), null);

        // Act
        var json = JsonSerializer.Serialize(payload, Options);

        // Assert
        json.Should().Be($$"""{"postId":42,"userId":"user-1","mediaId":"{{MediaGuid}}","parentId":null}""");
    }

    [Fact]
    public void GivenSerializedIds_WhenDeserialized_ThenEveryIdRoundTrips()
    {
        // Arrange
        var payload = new Payload(new PostId(42), new UserId("user-1"), new SampleGuidId(Guid.Parse(MediaGuid)), new PostId(7));
        var json = JsonSerializer.Serialize(payload, Options);

        // Act
        var roundTripped = JsonSerializer.Deserialize<Payload>(json, Options);

        // Assert
        roundTripped.Should().Be(payload);
    }

    [Theory]
    [InlineData($$"""{"postId":0,"userId":"user-1","mediaId":"{{MediaGuid}}","parentId":null}""")]
    [InlineData($$"""{"postId":-1,"userId":"user-1","mediaId":"{{MediaGuid}}","parentId":null}""")]
    [InlineData($$"""{"postId":42,"userId":"   ","mediaId":"{{MediaGuid}}","parentId":null}""")]
    [InlineData($$"""{"postId":42,"userId":null,"mediaId":"{{MediaGuid}}","parentId":null}""")]
    [InlineData("""{"postId":42,"userId":"user-1","mediaId":"00000000-0000-0000-0000-000000000000","parentId":null}""")]
    [InlineData($$"""{"postId":42,"userId":"user-1","mediaId":"{{MediaGuid}}","parentId":0}""")]
    public void GivenAnInvalidId_WhenDeserialized_ThenAJsonExceptionIsThrown(string json)
    {
        // Act
        var act = () => JsonSerializer.Deserialize<Payload>(json, Options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void GivenADictionaryKeyedByIds_WhenRoundTripped_ThenKeysAreWrittenAsPrimitivesAndRestored()
    {
        // Arrange
        var totals = new Dictionary<PostId, int> { [new PostId(42)] = 3 };

        // Act
        var json = JsonSerializer.Serialize(totals, Options);
        var roundTripped = JsonSerializer.Deserialize<Dictionary<PostId, int>>(json, Options);

        // Assert
        using (new AssertionScope())
        {
            json.Should().Be("""{"42":3}""");
            roundTripped.Should().Equal(totals);
        }
    }

    [Fact]
    public void GivenAnInvalidDictionaryKey_WhenDeserialized_ThenAJsonExceptionIsThrown()
    {
        // Act
        var act = () => JsonSerializer.Deserialize<Dictionary<UserId, int>>("""{" ":1}""", Options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData(typeof(PostId), true)]
    [InlineData(typeof(UserId), true)]
    [InlineData(typeof(SampleGuidId), true)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(Guid), false)]
    public void GivenAType_WhenTheFactoryIsAsked_ThenOnlyStronglyTypedIdsAreConvertible(Type type, bool expected)
    {
        // Act
        var convertible = new StronglyTypedIdJsonConverterFactory().CanConvert(type);

        // Assert
        convertible.Should().Be(expected);
    }
}
