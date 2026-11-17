namespace Eventix.Chassis.Core.Messaging;

/// <summary>
/// Enqueues integration events into the Outbox table WITHIN the current
/// transaction. The actual publish to the broker happens later via OutboxWorker.
/// 
/// This is the interface your domain event handlers depend on.
/// It lives in Core so Application layers can use it without referencing Infrastructure.
/// </summary>
public interface IOutboxPublisher
{
    /// <summary>
    /// Stores the event in the Outbox. Does NOT call SaveChanges:
    /// the caller's transaction controls the commit.
    /// </summary>
    Task EnqueueAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}