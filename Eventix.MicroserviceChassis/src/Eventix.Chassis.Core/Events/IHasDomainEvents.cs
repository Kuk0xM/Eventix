namespace Eventix.Chassis.Core.Events;

/// <summary>
/// Implemented by aggregate roots that raise domain events.
/// The Chassis interceptor uses this to collect and dispatch events
/// within the same SaveChanges transaction.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}