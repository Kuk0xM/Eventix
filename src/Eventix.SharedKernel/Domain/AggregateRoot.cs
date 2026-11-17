using Eventix.Chassis.Core.Events;

namespace Eventix.SharedKernel.Domain;

/// <summary>
/// Base class for all Aggregate Roots.
/// Preserves the original public API (AddDomainEvent, CheckRule) while
/// implementing the Chassis IHasDomainEvents contract.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Read-only collection of domain events that have not yet been published.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Registers a domain event. Dispatched by the Chassis interceptor
    /// within the same SaveChanges transaction.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears pending domain events. Called by the Chassis interceptor after dispatch.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Validates a business rule and throws if broken.
    /// </summary>
    protected void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}