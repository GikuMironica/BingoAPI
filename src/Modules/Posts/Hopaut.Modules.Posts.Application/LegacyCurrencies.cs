using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Application;

/// <summary>
/// Today's wire encodes a currency as its index in the legacy list (<c>Currencies.json</c> at tag <c>pre-net10-baseline</c>)
/// until the Posts endpoints switch to ISO-4217 codes in plan Step 10.4. Index 8 was the obsolete Bulgarian code "BGL";
/// it maps to "BGN" (D-026).
/// </summary>
public static class LegacyCurrencies
{
    private static readonly string[] Codes =
    [
        "EUR", "USD", "CHF", "GBP", "TRY", "UAH", "SEK", "RON", "BGN", "RUB", "RSD", "PLN", "NOK",
        "MDL", "HUF", "HRK", "DKK", "CZK", "COP", "AUD", "CAD", "MXN", "BRL", "BAM", "ARS",
    ];

    public static readonly Error Required =
        Error.Validation("posts.event.currency_required", "A currency is required when an entrance price is given.");

    public static readonly Error Unknown =
        Error.Validation("posts.event.unknown_currency", $"The currency must be an index from 0 to {Codes.Length - 1}.");

    public static readonly Error InvalidPrice =
        Error.Validation("posts.event.invalid_entrance_price", "The entrance price must be a finite number.");

    public static Result<Currency> FromIndex(int? index) => index switch
    {
        null => Required,
        { } i when i >= 0 && i < Codes.Length => Currency.Create(Codes[i]),
        _ => Unknown,
    };

    public static int? IndexOf(Currency currency) => Array.IndexOf(Codes, currency.Value) is var index and >= 0 ? index : null;

    /// <summary>The fee from today's wire fields: no price means no fee; a price needs a known currency.</summary>
    public static Result<Money>? EntranceFee(double? price, int? currencyIndex)
    {
        if (price is not { } amount)
            return null;

        if (!double.IsFinite(amount))
            return InvalidPrice;

        var currency = FromIndex(currencyIndex);
        if (currency.IsFailure)
            return currency.Error;

        if (amount < 0)
            return MoneyErrors.Negative;

        return amount > (double)Money.MaxAmount ? MoneyErrors.TooLarge : Money.Create((decimal)amount, currency.Value);
    }
}
