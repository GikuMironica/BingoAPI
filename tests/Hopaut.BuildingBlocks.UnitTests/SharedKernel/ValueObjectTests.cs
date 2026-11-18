using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class ValueObjectTests
{
    [Fact]
    public void GivenTwoValueObjectsWithEqualComponents_WhenCompared_ThenTheyAreEqualAndShareAHashCode()
    {
        // Arrange
        var first = new Temperature(21.5m, "C");
        var second = new Temperature(21.5m, "C");

        // Act
        var equal = first == second;

        // Assert
        using (new AssertionScope())
        {
            equal.Should().BeTrue();
            first.Equals(second).Should().BeTrue();
            first.GetHashCode().Should().Be(second.GetHashCode());
        }
    }

    [Fact]
    public void GivenTwoValueObjectsWithDifferentComponents_WhenCompared_ThenTheyAreNotEqual()
    {
        // Arrange
        var celsius = new Temperature(21.5m, "C");
        var fahrenheit = new Temperature(21.5m, "F");

        // Act
        var notEqual = celsius != fahrenheit;

        // Assert
        notEqual.Should().BeTrue();
    }

    [Fact]
    public void GivenValueObjectsOfDifferentTypesWithEqualComponents_WhenCompared_ThenTheyAreNotEqual()
    {
        // Arrange
        ValueObject temperature = new Temperature(1m, "C");
        ValueObject distance = new Distance(1m, "C");

        // Act
        var equal = temperature.Equals(distance);

        // Assert
        equal.Should().BeFalse();
    }

    private sealed class Temperature(decimal amount, string unit) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents() => [amount, unit];
    }

    private sealed class Distance(decimal amount, string unit) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents() => [amount, unit];
    }
}
