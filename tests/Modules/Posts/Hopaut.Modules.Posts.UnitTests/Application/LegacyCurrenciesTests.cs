using AwesomeAssertions.Execution;
using Hopaut.Modules.Posts.Application;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.UnitTests.Application;

public sealed class LegacyCurrenciesTests
{
    [Theory]
    [InlineData(0, "EUR")]
    [InlineData(2, "CHF")]
    [InlineData(8, "BGN")]
    [InlineData(24, "ARS")]
    public void FromIndex_WithALegacyIndex_ReturnsTheIsoCodeAndMapsBack(int index, string code)
    {
        // Act
        var currency = LegacyCurrencies.FromIndex(index);

        // Assert
        using (new AssertionScope())
        {
            currency.Value.Value.Should().Be(code);
            LegacyCurrencies.IndexOf(currency.Value).Should().Be(index);
        }
    }

    [Theory]
    [InlineData(null, "posts.event.currency_required")]
    [InlineData(-1, "posts.event.unknown_currency")]
    [InlineData(25, "posts.event.unknown_currency")]
    public void FromIndex_WithoutAKnownIndex_Fails(int? index, string code)
    {
        // Act
        var currency = LegacyCurrencies.FromIndex(index);

        // Assert
        currency.Error.Code.Should().Be(code);
    }

    [Fact]
    public void IndexOf_ACurrencyOutsideTheLegacyList_IsNull()
    {
        // Act
        var index = LegacyCurrencies.IndexOf(Currency.Create("JPY").Value);

        // Assert
        index.Should().BeNull();
    }

    [Fact]
    public void EntranceFee_WithoutAPrice_IsNoFeeEvenWithACurrency()
    {
        // Act
        var fee = LegacyCurrencies.EntranceFee(null, 3);

        // Assert
        fee.Should().BeNull();
    }

    [Fact]
    public void EntranceFee_WithAPriceAndCurrency_IsRoundedMoney()
    {
        // Act
        var fee = LegacyCurrencies.EntranceFee(12.345, 2);

        // Assert
        fee!.Value.Should().Be(Money.Create(12.35m, Currency.Create("CHF").Value).Value);
    }

    [Theory]
    [InlineData(double.NaN, 0, "posts.event.invalid_entrance_price")]
    [InlineData(double.PositiveInfinity, 0, "posts.event.invalid_entrance_price")]
    [InlineData(-1, 0, "shared.money.negative")]
    [InlineData(1e12, 0, "shared.money.too_large")]
    [InlineData(5, 99, "posts.event.unknown_currency")]
    public void EntranceFee_WithAnInvalidPriceOrCurrency_Fails(double price, int currency, string code)
    {
        // Act
        var fee = LegacyCurrencies.EntranceFee(price, currency);

        // Assert
        fee!.Error.Code.Should().Be(code);
    }
}
