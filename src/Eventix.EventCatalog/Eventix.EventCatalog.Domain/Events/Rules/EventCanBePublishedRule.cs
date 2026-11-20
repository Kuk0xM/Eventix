using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

public sealed class EventCanBePublishedRule(EventStatus status) : IBusinessRule
{
    public string Message => "Cancelled events cannot be published.";

    public bool IsBroken() => status == EventStatus.Cancelled;
}