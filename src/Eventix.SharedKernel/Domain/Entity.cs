namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Base entity: identity only.
/// Domain events live in AggregateRoot (only aggregates raise events).
/// </summary>
public abstract class Entity<TId>
{
    public TId Id { get; protected set; }

    /// <summary>
    /// Parameterless constructor for EF Core hydration.
    /// </summary>
    protected Entity()
    {
        Id = default!;
    }

    /// <summary>
    /// Constructor for domain code.
    /// </summary>
    protected Entity(TId id)
    {
        Id = id;
    }
}