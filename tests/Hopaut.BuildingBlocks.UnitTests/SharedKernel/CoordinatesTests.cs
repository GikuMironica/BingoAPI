using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class CoordinatesTests
{
    [Theory]
    [InlineData(52.52, 13.405)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void GivenAPositionWithinWgs84_WhenCreated_ThenBothAxesAreKept(double latitude, double longitude)
    {
        // Act
        var result = Coordinates.Create(latitude, longitude);

        // Assert
        using (new AssertionScope())
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Latitude.Should().Be(latitude);
            result.Value.Longitude.Should().Be(longitude);
        }
    }

    [Theory]
    [InlineData(90.000_1, 0, "shared.coordinates.latitude_out_of_range")]
    [InlineData(-90.000_1, 0, "shared.coordinates.latitude_out_of_range")]
    [InlineData(double.NaN, 0, "shared.coordinates.latitude_out_of_range")]
    [InlineData(double.PositiveInfinity, 0, "shared.coordinates.latitude_out_of_range")]
    [InlineData(0, 180.000_1, "shared.coordinates.longitude_out_of_range")]
    [InlineData(0, -180.000_1, "shared.coordinates.longitude_out_of_range")]
    [InlineData(0, double.NegativeInfinity, "shared.coordinates.longitude_out_of_range")]
    public void GivenAPositionOutsideWgs84_WhenCreated_ThenTheOffendingAxisIsReported(double latitude, double longitude, string code)
    {
        // Act
        var result = Coordinates.Create(latitude, longitude);

        // Assert
        result.Error.Code.Should().Be(code);
    }

    [Fact]
    public void GivenTwoEqualPositions_WhenCompared_ThenTheyAreEqual()
    {
        // Act
        var first = Coordinates.Create(47.37, 8.54).Value;
        var second = Coordinates.Create(47.37, 8.54).Value;

        // Assert
        first.Should().Be(second);
    }
}
