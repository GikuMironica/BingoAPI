using AwesomeAssertions.Execution;
using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.UnitTests.SharedKernel;

public sealed class MoneyTests
{
    [Theory]
    [InlineData("EUR", "EUR")]
    [InlineData(" chf ", "CHF")]
    [InlineData("usd", "USD")]
    public void GivenAThreeLetterCode_WhenACurrencyIsCreated_ThenItIsTrimmedAndUpperCased(string input, string expected)
    {
        // Act
        var result = Currency.Create(input);

        // Assert
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("E1R")]
    [InlineData("ÉUR")]
    public void GivenSomethingOtherThanThreeAsciiLetters_WhenACurrencyIsCreated_ThenItFails(string? input)
    {
        // Act
        var result = Currency.Create(input);

        // Assert
        result.Error.Code.Should().Be("shared.money.invalid_currency");
    }

    [Theory]
    [InlineData("12.345", "12.35")]
    [InlineData("12.344", "12.34")]
    [InlineData("0", "0")]
    [InlineData("99999999.99", "99999999.99")]
    public void GivenANonNegativeAmount_WhenMoneyIsCreated_ThenItIsRoundedToCents(string amount, string expected)
    {
        // Act
        var result = Money.Create(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture), Currency.Euro);

        // Assert
        using (new AssertionScope())
        {
            result.Value.Amount.Should().Be(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture));
            result.Value.Currency.Should().Be(Currency.Euro);
        }
    }

    [Theory]
    [InlineData("-0.01", "shared.money.negative")]
    [InlineData("100000000", "shared.money.too_large")]
    public void GivenANegativeOrTooLargeAmount_WhenMoneyIsCreated_ThenItFails(string amount, string code)
    {
        // Act
        var result = Money.Create(decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture), Currency.Euro);

        // Assert
        result.Error.Code.Should().Be(code);
    }

    [Fact]
    public void GivenTheSameAmountAndCurrency_WhenCompared_ThenTheyAreEqualAndPrintable()
    {
        // Act
        var first = Money.Create(12.5m, Currency.Create("eur").Value).Value;
        var second = Money.Create(12.50m, Currency.Euro).Value;

        // Assert
        using (new AssertionScope())
        {
            first.Should().Be(second);
            first.ToString().Should().Be("12.50 EUR");
            Money.Zero(Currency.Euro).Amount.Should().Be(0m);
        }
    }
}
