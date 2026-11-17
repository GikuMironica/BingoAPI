using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Text;

namespace Hopaut.SharedKernel;

/// <summary>The number of items in one page: 1 to <see cref="Max"/>, <see cref="Default"/> when the client sends none (rest-api-standards §6).</summary>
public sealed record PageSize : ISingleValueObject<PageSize, int>
{
    public const int Max = 100;

    private PageSize(int value) => Value = value;

    public static PageSize Default { get; } = new(20);

    public int Value { get; }

    public static Result<PageSize> Create(int value) =>
        value is >= 1 and <= Max ? new PageSize(value) : PagingErrors.PageSizeOutOfRange;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// An opaque paging position: base64url text (no padding) of at most <see cref="MaxLength"/> characters. A read model
/// encodes its position with <see cref="Encode"/> and reads it back with <see cref="Decode"/>; the format of the position
/// belongs to that read model, which treats an unreadable position as an invalid cursor.
/// </summary>
public sealed record Cursor : ISingleValueObject<Cursor, string>
{
    public const int MaxLength = 512;

    private static readonly SearchValues<char> Base64UrlCharacters =
        SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_");

    private Cursor(string value) => Value = value;

    public string Value { get; }

    public static Result<Cursor> Create(string? value)
    {
        if (string.IsNullOrEmpty(value)
            || value.Length > MaxLength
            || value.AsSpan().ContainsAnyExcept(Base64UrlCharacters)
            || !Base64Url.IsValid(value))
        {
            return PagingErrors.InvalidCursor;
        }

        return new Cursor(value);
    }

    public static Cursor Encode(string position)
    {
        ArgumentException.ThrowIfNullOrEmpty(position);

        var value = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(position));
        return value.Length <= MaxLength
            ? new Cursor(value)
            : throw new ArgumentException($"The encoded position is longer than {MaxLength} characters.", nameof(position));
    }

    public string Decode() => Encoding.UTF8.GetString(Base64Url.DecodeFromChars(Value));

    public override string ToString() => Value;
}

public static class PagingErrors
{
    public static readonly Error PageSizeOutOfRange =
        Error.Validation("shared.page_size.out_of_range", $"The page size must be between 1 and {PageSize.Max}.");

    public static readonly Error InvalidCursor =
        Error.Validation("shared.cursor.invalid", "The cursor is not valid.");
}
