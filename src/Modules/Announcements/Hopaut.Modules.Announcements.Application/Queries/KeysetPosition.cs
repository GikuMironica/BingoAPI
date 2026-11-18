using System.Globalization;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Application.Queries;

/// <summary>
/// The opaque form of the module's keyset positions, a UTC time and a positive ID ("ticks:id" inside a SharedKernel
/// <see cref="Cursor"/>); an unreadable position is an invalid cursor.
/// </summary>
internal static class KeysetPosition
{
    public static Cursor Encode(DateTimeOffset time, int id) =>
        Cursor.Encode(string.Create(CultureInfo.InvariantCulture, $"{time.UtcTicks}:{id}"));

    public static Result<(DateTimeOffset Time, int Id)> Decode(string? value)
    {
        var cursor = Cursor.Create(value);
        if (cursor.IsFailure)
            return cursor.Error;

        var parts = cursor.Value.Decode().Split(':');
        return parts.Length == 2
            && long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
            && ticks <= DateTimeOffset.MaxValue.UtcTicks
            && int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            && id > 0
                ? (new DateTimeOffset(ticks, TimeSpan.Zero), id)
                : PagingErrors.InvalidCursor;
    }
}
