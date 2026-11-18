using System.Globalization;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// Why a post or a user is reported: a reason code from <see cref="Min"/> to <see cref="Max"/>, the range the legacy API
/// accepted for both kinds of report. The legacy never named the codes; plan Step 10.8 agrees their names with BingoMobile
/// (decision D-036, finding F-077).
/// </summary>
public sealed record ReportReason : ISingleValueObject<ReportReason, int>
{
    public const int Min = 1;
    public const int Max = 6;

    private ReportReason(int value) => Value = value;

    public int Value { get; }

    public static Result<ReportReason> Create(int value) =>
        value is >= Min and <= Max ? new ReportReason(value) : ReportErrors.ReasonOutOfRange;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
