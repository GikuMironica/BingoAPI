using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Posts.Application;
using Hopaut.Modules.Posts.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Posts.Infrastructure;

/// <summary>
/// Finds tags by name and creates the missing ones. Two requests creating the same new tag at once collide on the unique
/// name index; the loser's unit of work answers <c>general.concurrency_conflict</c> and a retry finds the tag (D-027).
/// </summary>
public sealed class TagRepository(PostsModuleDbContext db) : ITagRepository
{
    public async Task<IReadOnlyList<Tag>> GetOrCreateAsync(IReadOnlyCollection<TagName> names, CancellationToken ct = default)
    {
        if (names.Count == 0)
            return [];

        var tags = await db.Tags.Where(tag => names.Contains(tag.Name)).ToListAsync(ct);
        foreach (var name in names.Where(name => !tags.Exists(tag => tag.Name == name)).ToList())
        {
            var id = TagId.From(await db.Database.NextSequenceValueAsync(PostsModuleDbContext.SchemaName, PostsModuleDbContext.TagIdSequence, ct));
            var tag = Tag.Create(id, name);
            db.Tags.Add(tag);
            tags.Add(tag);
        }

        return tags;
    }
}
