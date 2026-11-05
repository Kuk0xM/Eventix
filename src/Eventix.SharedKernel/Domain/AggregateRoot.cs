namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Base class for all Aggregate Roots.
/// Handles the collection of Domain Events and validates business rules.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only collection of domain events that have not yet been published.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Registers a new domain event to be published after persistence.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears the list of events after they have been published.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Validates a business rule. If broken, throws an exception.
    /// </summary>
    protected void CheckRule(IBusinessRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (rule.IsBroken())
            throw new BusinessRuleValidationException(rule);
    }
}