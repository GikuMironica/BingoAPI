using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// What the user-report rules need to know about the reported user from the reporter's side, gathered by the handler: when
/// the reporter last reported the user in a report that is still open (null when there is none).
/// </summary>
public sealed record ReportedUser(UserId UserId, DateTimeOffset? ReporterLastOpenReportAt)
{
    /// <summary>The longest user ID: the key length of the Identity module, which every module stores user IDs in.</summary>
    public const int MaxIdLength = 450;

    /// <summary>Whether <paramref name="userId"/> can identify a user: not blank and at most <see cref="MaxIdLength"/> characters.</summary>
    public static bool IsUserId(UserId userId) =>
        StronglyTypedId.IsValid(userId.Value) && userId.Value.Length <= MaxIdLength;
}
