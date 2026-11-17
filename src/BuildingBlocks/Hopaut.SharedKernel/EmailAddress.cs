using System.Net.Mail;

namespace Hopaut.SharedKernel;

/// <summary>An e-mail address, trimmed and lower-cased: one '@', a local part of at most 64 characters, a dotted domain, at most 254 characters.</summary>
public sealed record EmailAddress : ISingleValueObject<EmailAddress, string>
{
    public const int MaxLength = 254;
    private const int MaxLocalPartLength = 64;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string? value)
    {
        var address = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(address))
            return EmailAddressErrors.Required;

        if (address.Length > MaxLength)
            return EmailAddressErrors.TooLong;

        return IsWellFormed(address) ? new EmailAddress(address) : EmailAddressErrors.Invalid;
    }

    public override string ToString() => Value;

    private static bool IsWellFormed(string address)
    {
        var at = address.IndexOf('@');
        if (at is <= 0 or > MaxLocalPartLength || at != address.LastIndexOf('@') || address.Any(char.IsWhiteSpace))
            return false;

        var domain = address[(at + 1)..];
        return domain.Contains('.')
            && !domain.StartsWith('.')
            && !domain.EndsWith('.')
            && !domain.Contains("..", StringComparison.Ordinal)
            && MailAddress.TryCreate(address, out var parsed)
            && parsed.Address == address;
    }
}

public static class EmailAddressErrors
{
    public static readonly Error Required =
        Error.Validation("shared.email_address.required", "An e-mail address is required.");

    public static readonly Error TooLong =
        Error.Validation("shared.email_address.too_long", $"An e-mail address can have at most {EmailAddress.MaxLength} characters.");

    public static readonly Error Invalid =
        Error.Validation("shared.email_address.invalid", "The e-mail address is not valid.");
}
