using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>
/// A tag shared by posts. It is its own aggregate: posts reference it by <see cref="TagId"/> only, its ID comes from the
/// tags sequence before it is created, and its <see cref="Name"/> is unique.
/// </summary>
public sealed class Tag : AggregateRoot<TagId>
{
    private Tag() { } // EF Core

    public TagName Name { get; private set; }

    public static Tag Create(TagId id, TagName name)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(name);

        return new Tag { Id = id, Name = name };
    }
}
