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

    private sealed class Sample : Entity<int>
    {
        private Sample(int id) => Id = id;

        public static Sample WithId(int id) => new(id);
    }
}
