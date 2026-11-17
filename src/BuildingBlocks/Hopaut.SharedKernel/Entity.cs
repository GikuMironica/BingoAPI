namespace Hopaut.SharedKernel;

/// <summary>
/// Entities are equal when they have the same type and identity. An entity whose key is still the default (not yet
/// assigned by the database) is transient and equal only to itself, so its hash code changes once the key is assigned.
/// </summary>
public abstract class Entity<TId> where TId : struct
{
    public TId Id { get; protected set; }

    private bool IsTransient => EqualityComparer<TId>.Default.Equals(Id, default);

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other
        && other.GetType() == GetType()
        && (ReferenceEquals(this, other)
            || (!IsTransient && !other.IsTransient && EqualityComparer<TId>.Default.Equals(Id, other.Id)));

    public override int GetHashCode() => IsTransient ? base.GetHashCode() : HashCode.Combine(GetType(), Id);
}
