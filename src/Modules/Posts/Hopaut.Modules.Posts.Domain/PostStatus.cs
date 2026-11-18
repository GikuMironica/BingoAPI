namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// Whether a post is listed (stored in the former <c>ActiveFlag</c> values). An ended post stays <see cref="Active"/>;
/// "inactive" lists are derived from the end of its schedule, as in the legacy API.
/// </summary>
public enum PostStatus
{
    Disabled = 0,
    Active = 1,
}
