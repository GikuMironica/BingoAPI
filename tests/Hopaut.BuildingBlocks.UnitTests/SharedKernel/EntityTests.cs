using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class EntityTests
{
    [Fact]
    public void GivenTwoPersistedEntitiesWithTheSameId_WhenCompared_ThenTheyAreEqual()
    {
        // Arrange
        var first = Sample.WithId(42);
        var second = Sample.WithId(42);

        // Act
        var equal = first.Equals(second);

        // Assert
        using (new AssertionScope())
        {
            equal.Should().BeTrue();
            first.GetHashCode().Should().Be(second.GetHashCode());
        }
    }

    [Fact]
    public void GivenTwoPersistedEntitiesWithDifferentIds_WhenCompared_ThenTheyAreNotEqual()
    {
        // Arrange
        var first = Sample.WithId(1);
        var second = Sample.WithId(2);

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void GivenTwoTransientEntities_WhenCompared_ThenTheyAreNotEqual()
    {
        // Arrange
        var first = Sample.Transient();
        var second = Sample.Transient();

        // Act
        var equal = first.Equals(second);

        // Assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void GivenATransientEntity_WhenComparedWithItself_ThenItIsEqual()
    {
        // Arrange
        var entity = Sample.Transient();

        // Act
        var equal = entity.Equals(entity);

        // Assert
        equal.Should().BeTrue();
    }

    [Fact]
    public void GivenEntitiesOfDifferentTypesWithTheSameId_WhenCompared_ThenTheyAreNotEqual()
    {
        // Arrange
        var sample = Sample.WithId(42);
        var other = OtherSample.WithId(42);

        // Act
        var equal = sample.Equals(other);

        // Assert
        equal.Should().BeFalse();
    }

    private sealed class Sample : Entity<int>
    {
        private Sample(int id) => Id = id;

        public static Sample WithId(int id) => new(id);

        public static Sample Transient() => new(default);
    }

    private sealed class OtherSample : Entity<int>
    {
        private OtherSample(int id) => Id = id;

        public static OtherSample WithId(int id) => new(id);
    }
}
