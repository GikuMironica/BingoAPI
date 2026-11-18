using Hopaut.Modules.Posts.Domain;

namespace Hopaut.Modules.Posts.Application;

/// <summary>Tag aggregate store. New tags are saved by the Posts unit of work, never by the repository.</summary>
public interface ITagRepository
{
    /// <summary>The tags with these names; missing ones are created with IDs from the tags sequence.</summary>
    Task<IReadOnlyList<Tag>> GetOrCreateAsync(IReadOnlyCollection<TagName> names, CancellationToken ct = default);
}
