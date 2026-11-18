using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Application;

/// <summary>Turns the optional report inputs as sent into value objects; a blank message means the report has none.</summary>
internal static class ReportInput
{
    public static Result<ReportMessage>? Message(string? message) =>
        string.IsNullOrWhiteSpace(message) ? null : ReportMessage.Create(message);
}
