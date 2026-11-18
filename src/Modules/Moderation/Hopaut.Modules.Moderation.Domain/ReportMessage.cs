using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// What a reporter adds to the reason: trimmed, 1 to <see cref="MaxLength"/> characters (UTF-16 code units, like the legacy
/// limit of the other free-text reports). A report may come without one (decision D-036).
/// </summary>
public sealed record ReportMessage : ISingleValueObject<ReportMessage, string>
{
    public const int MaxLength = 300;

    private ReportMessage(string value) => Value = value;

    public string Value { get; }

    public static Result<ReportMessage> Create(string? value)
    {
        var message = value?.Trim();
        if (string.IsNullOrEmpty(message))
            return ReportErrors.MessageEmpty;

        return message.Length <= MaxLength ? new ReportMessage(message) : ReportErrors.MessageTooLong;
    }

    public override string ToString() => Value;
}
