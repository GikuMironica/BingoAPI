using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class NumericValueObjectTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void GivenAScoreFromOneToFive_WhenARatingValueIsCreated_ThenItIsKept(int score)
    {
        // Act
        var result = RatingValue.Create(score);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Value.Should().Be(score);
            ((int)result.Value).Should().Be(score);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void GivenAScoreOutsideOneToFive_WhenARatingValueIsCreated_ThenItFailsInsteadOfThrowing(int score)
    {
        // Act
        var result = RatingValue.Create(score);

        // Assert
        result.Error.Code.Should().Be("shared.rating_value.out_of_range");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    public void GivenAPositiveNumber_WhenACapacityIsCreated_ThenItIsKept(int slots)
    {
        // Act
        var result = Capacity.Create(slots);

        // Assert
        result.Value.Value.Should().Be(slots);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void GivenZeroOrLessSlots_WhenACapacityIsCreated_ThenItFails(int slots)
    {
        // Act
        var result = Capacity.Create(slots);

        // Assert
        result.Error.Code.Should().Be("shared.capacity.not_positive");
    }

    [Theory]
    [InlineData(RadiusMeters.Min, true)]
    [InlineData(RadiusMeters.Max, true)]
    [InlineData(RadiusMeters.Min - 1, false)]
    [InlineData(RadiusMeters.Max + 1, false)]
    public void GivenARadius_WhenCreated_ThenOnlyOneToFifteenKilometersAreAccepted(int meters, bool accepted)
    {
        // Act
        var result = RadiusMeters.Create(meters);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().Be(accepted);
            (result.IsSuccess ? null : result.Error.Code).Should().Be(accepted ? null : "shared.radius.out_of_range");
        }
    }

    [Fact]
    public void GivenTheDefaults_WhenRead_ThenTheyMatchTheDocumentedValues()
    {
        // Act
        var radius = RadiusMeters.Default.Value;
        var pageSize = PageSize.Default.Value;

        // Assert
        using (new AssertionScope())
        {
            radius.Should().Be(10_000);
            pageSize.Should().Be(20);
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(PageSize.Max, true)]
    [InlineData(0, false)]
    [InlineData(PageSize.Max + 1, false)]
    public void GivenAPageSize_WhenCreated_ThenOnlyOneToOneHundredIsAccepted(int size, bool accepted)
    {
        // Act
        var result = PageSize.Create(size);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().Be(accepted);
            (result.IsSuccess ? null : result.Error.Code).Should().Be(accepted ? null : "shared.page_size.out_of_range");
        }
    }
}
