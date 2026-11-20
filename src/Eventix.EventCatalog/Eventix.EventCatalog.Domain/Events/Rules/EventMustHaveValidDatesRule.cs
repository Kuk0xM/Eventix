using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

public class EventMustHaveValidDatesRule : IBusinessRule
{
    private readonly EventDate _startDate;
    private readonly EventDate _endDate;

    public EventMustHaveValidDatesRule(EventDate startDate, EventDate endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public bool IsBroken() => _endDate.Value <= _startDate.Value;

    public string Message => "Event end date must be after start date.";
}