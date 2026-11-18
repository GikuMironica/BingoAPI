using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>Expected failures of the report values both report aggregates share; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class ReportErrors
{
    public static readonly Error ReasonOutOfRange =
        Error.Validation("moderation.reason.out_of_range", $"The reason must be between {ReportReason.Min} and {ReportReason.Max}.");

    public static readonly Error MessageEmpty =
        Error.Validation("moderation.message.empty", "The message cannot be blank.");

    public static readonly Error MessageTooLong =
        Error.Validation("moderation.message.too_long", $"The message can have at most {ReportMessage.MaxLength} characters.");
}
