using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>Repeat settings of a post (no fields yet; kept so the table and the one-to-one relationship survive).</summary>
public sealed class RepeatableProperty
{
    private RepeatableProperty() { } // EF Core

    public PostId PostId { get; private set; }
}
