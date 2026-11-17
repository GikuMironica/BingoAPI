using Hopaut.SharedKernel;

namespace Hopaut.Modules.Posts.Domain;

/// <summary>A tag name shared by posts; posts reference it by <see cref="TagId"/>.</summary>
public sealed class Tag
{
    private Tag() { } // EF Core

    public TagId Id { get; private set; }

    public string TagName { get; private set; }
}
