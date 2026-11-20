using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

public sealed class EventCanBeCancelledRule(EventStatus status) : IBusinessRule
{
    public string Message => "Cancelled events cannot be cancelled again.";

    public bool IsBroken() => status == EventStatus.Cancelled;
}