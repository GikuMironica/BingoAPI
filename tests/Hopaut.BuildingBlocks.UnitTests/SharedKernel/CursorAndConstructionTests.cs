using System.Reflection;
using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class CursorTests
{
    [Fact]
    public void GivenAReadModelPosition_WhenEncodedAndParsedBack_ThenTheSamePositionIsDecoded()
    {
        // Arrange
        const string position = "2030-01-09T18:00:00Z|42";

        // Act
        var encoded = Cursor.Encode(position);
        var parsed = Cursor.Create(encoded.Value);

        // Assert
        using (new AssertionScope())
        {
            encoded.Value.Should().MatchRegex("^[A-Za-z0-9_-]+$");
            parsed.Value.Should().Be(encoded);
            parsed.Value.Decode().Should().Be(position);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc=")]
    [InlineData("ab+c")]
    [InlineData("ab/c")]
    [InlineData("ab c")]
    [InlineData("a")]
    public void GivenTextThatIsNotUnpaddedBase64Url_WhenACursorIsCreated_ThenItFails(string? input)
    {
        // Act
        var result = Cursor.Create(input);

        // Assert
        result.Error.Code.Should().Be("shared.cursor.invalid");
    }

    [Fact]
    public void GivenACursorLongerThan512Characters_WhenCreated_ThenItFails()
    {
        // Act
        var result = Cursor.Create(new string('A', Cursor.MaxLength + 4));

        // Assert
        result.Error.Code.Should().Be("shared.cursor.invalid");
    }
}

public sealed class ValueObjectConstructionTests
{
    private static readonly Type[] Listed =
    [
        typeof(Coordinates), typeof(TimeRange), typeof(Currency), typeof(Money), typeof(RatingValue), typeof(EmailAddress),
        typeof(PostTitle), typeof(PostDescription), typeof(Capacity), typeof(RadiusMeters), typeof(PageSize), typeof(Cursor),
        typeof(MediaKey),
    ];

    public static TheoryData<Type> ValueObjects()
    {
        var data = new TheoryData<Type>();
        foreach (var type in Listed)
        {
            data.Add(type);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ValueObjects))]
    public void GivenAValueObject_WhenItsConstructorsAreInspected_ThenNoneIsPublic(Type valueObject)
    {
        // Act
        var publicConstructors = valueObject.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        // Assert
        publicConstructors.Should().BeEmpty("only the validating factory may create {0}", valueObject.Name);
    }

    [Fact]
    public void GivenTheSharedKernel_WhenSingleValueObjectsAreListed_ThenEachIsCoveredByTheConstructorCheck()
    {
        // Act
        var singleValueObjects = typeof(ISingleValueObject<,>).Assembly.GetTypes()
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISingleValueObject<,>)));

        // Assert
        singleValueObjects.Should().OnlyContain(type => Listed.Contains(type));
    }
}
