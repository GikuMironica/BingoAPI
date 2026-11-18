using Hopaut.SharedKernel;

namespace Hopaut.Modules.Announcements.Domain;

/// <summary>
/// The text of an announcement: trimmed, 1 to <see cref="MaxLength"/> characters. The legacy API applied this rule when an
/// announcement was created; it now applies to edits too (decision D-033).
/// </summary>
public sealed record AnnouncementMessage : ISingleValueObject<AnnouncementMessage, string>
{
    public const int MaxLength = 500;

    private AnnouncementMessage(string value) => Value = value;

    public string Value { get; }

    public static Result<AnnouncementMessage> Create(string? value)
    {
        var message = value?.Trim();
        if (string.IsNullOrEmpty(message))
            return AnnouncementErrors.MessageEmpty;

        return message.Length <= MaxLength ? new AnnouncementMessage(message) : AnnouncementErrors.MessageTooLong;
    }

    public override string ToString() => Value;
}
