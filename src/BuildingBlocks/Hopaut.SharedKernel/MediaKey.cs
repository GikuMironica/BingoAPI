using System.Buffers;

namespace Hopaut.SharedKernel;

/// <summary>
/// The key of a stored media object (for example <c>assets/posts/0193….webp</c>): '/'-separated segments of ASCII letters,
/// digits, '.', '_' and '-', at most <see cref="MaxLength"/> characters, without empty, '.' or '..' segments, so a key can
/// never leave its prefix.
/// </summary>
public sealed record MediaKey : ISingleValueObject<MediaKey, string>
{
    public const int MaxLength = 1024;

    private static readonly SearchValues<char> SegmentCharacters =
        SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-");

    private MediaKey(string value) => Value = value;

    public string Value { get; }

    public static Result<MediaKey> Create(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxLength)
            return MediaKeyErrors.Invalid;

        foreach (var segment in value.Split('/'))
        {
            if (segment is "" or "." or ".." || segment.AsSpan().ContainsAnyExcept(SegmentCharacters))
                return MediaKeyErrors.Invalid;
        }

        return new MediaKey(value);
    }

    public override string ToString() => Value;
}

public static class MediaKeyErrors
{
    public static readonly Error Invalid =
        Error.Validation("shared.media_key.invalid", "The media key is not valid.");
}
