using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.Infrastructure;

public sealed class ValueObjectConverterTests
{
    [Fact]
    public void GivenAValueObject_WhenConvertedToTheColumnAndBack_ThenItRoundTrips()
    {
        // Arrange
        var converter = new ValueObjectConverter<EmailAddress, string>();
        var address = EmailAddress.Create("alice@hopaut.app").Value;

        // Act
        var column = converter.ConvertToProvider(address);
        var restored = converter.ConvertFromProvider(column);

        // Assert
        restored.Should().Be(address);
    }

    [Fact]
    public void GivenAStoredValueTheFactoryRejects_WhenRead_ThenItThrowsInsteadOfCreatingAnInvalidObject()
    {
        // Arrange
        var converter = new ValueObjectConverter<RatingValue, int>();

        // Act
        var act = () => converter.ConvertFromProvider(9);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*RatingValue*shared.rating_value.out_of_range*");
    }
}
