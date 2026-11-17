using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Hopaut.BuildingBlocks.Infrastructure.Persistence;

public static class SequenceExtensions
{
    /// <summary>
    /// Next value of a module-owned Postgres sequence (<c>HasSequence&lt;int&gt;</c> in the module's model). Aggregates with
    /// <c>int</c> keys take their ID from it before they are created, so the ID is known before any domain event is raised
    /// and before the unit of work saves (clean-architecture-ddd skill §2).
    /// </summary>
    public static Task<int> NextSequenceValueAsync(
        this DatabaseFacade database, string schema, string sequence, CancellationToken cancellationToken)
    {
        var qualifiedName = $"{schema}.{sequence}";
        return database
            .SqlQuery<int>($"SELECT nextval({qualifiedName}::regclass)::int AS \"Value\"")
            .SingleAsync(cancellationToken);
    }
}
