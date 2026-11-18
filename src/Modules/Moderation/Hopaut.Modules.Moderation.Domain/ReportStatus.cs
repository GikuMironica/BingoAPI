namespace Hopaut.Modules.Moderation.Domain;

/// <summary>Where a report stands: open until an administrator resolves it (the legacy API deleted handled reports instead).</summary>
public enum ReportStatus
{
    Open = 0,
    Resolved = 1,
}
