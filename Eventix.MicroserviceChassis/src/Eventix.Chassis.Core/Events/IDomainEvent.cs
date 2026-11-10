using MediatR;

namespace Eventix.Chassis.Core.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something important that happened WITHIN a single
/// bounded context (unlike integration events which cross boundaries).
/// 
/// Inherits from MediatR's INotification to enable in-process pub/sub.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// When the domain event occurred (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base implementation with default timestamp.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}