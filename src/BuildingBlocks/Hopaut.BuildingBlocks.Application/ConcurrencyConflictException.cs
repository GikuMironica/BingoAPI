using Hopaut.SharedKernel;

namespace Hopaut.BuildingBlocks.Application;

/// <summary>
/// Thrown by a module's unit of work when its changes collide with a concurrent change: an aggregate's concurrency token
/// (<c>xmin</c>) no longer matches, or a unique constraint was taken in the meantime. Nothing was saved.
/// <see cref="Behaviors.UnitOfWorkBehavior{TMessage,TResponse}"/> turns it into <see cref="UnitOfWorkErrors.ConcurrencyConflict"/>.
/// </summary>
public sealed class ConcurrencyConflictException(string message, Exception innerException) : Exception(message, innerException);

public static class UnitOfWorkErrors
{
    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "general.concurrency_conflict",
        "The data was changed by another request at the same time. Reload it and try again.");
}
