using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// What the post-report rules need to know about the reported post from the reporter's side, gathered by the handler: who
/// hosts it (Posts.Contracts) and whether the reporter already has an open report on it.
/// </summary>
public sealed record ReportedPost(PostId PostId, UserId HostId, bool ReporterHasOpenReport);
