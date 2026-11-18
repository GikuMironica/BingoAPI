using System.Globalization;

namespace Hopaut.SharedKernel;

/// <summary>An ISO-4217 currency code (<see cref="Value"/>): three ASCII letters, upper-cased.</summary>
public sealed record Currency : ISingleValueObject<Currency, string>
{
    private Currency(string code) => Value = code;

    public static Currency Euro { get; } = new("EUR");

    public string Value { get; }

    public static Result<Currency> Create(string? value)
    {
        var code = value?.Trim().ToUpperInvariant();
        return code is { Length: 3 } && code.All(char.IsAsciiLetterUpper)
            ? new Currency(code)
            : MoneyErrors.InvalidCurrency;
    }

    public override string ToString() => Value;
}

/// <summary>A non-negative amount of a currency, rounded to two decimals (midpoints away from zero).</summary>
public sealed record Money
{
    public const decimal MaxAmount = 99_999_999.99m;

    private Money(decimal amount, Currency currency) => (Amount, Currency) = (amount, currency);

    public decimal Amount { get; }

    public Currency Currency { get; }

    public static Result<Money> Create(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        var rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (rounded < 0)
            return MoneyErrors.Negative;

        if (rounded > MaxAmount)
            return MoneyErrors.TooLarge;

        return new Money(rounded, currency);
    }

    public static Money Zero(Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        return new Money(0m, currency);
    }

    public override string ToString() => $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";
}

public static class MoneyErrors
{
    public static readonly Error InvalidCurrency =
        Error.Validation("shared.money.invalid_currency", "The currency must be a three-letter ISO-4217 code.");

    public static readonly Error Negative =
        Error.Validation("shared.money.negative", "The amount cannot be negative.");

    public static readonly Error TooLarge =
        Error.Validation("shared.money.too_large", $"The amount cannot exceed {Money.MaxAmount.ToString(CultureInfo.InvariantCulture)}.");
}
