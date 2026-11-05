using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Rule: An event must have a valid name.
/// </summary>
public class EventMustHaveNameRule : IBusinessRule
{
    private readonly string _name;

    public EventMustHaveNameRule(string name) => _name = name;

    public bool IsBroken() => string.IsNullOrWhiteSpace(_name);

    public string Message => "Event name cannot be empty.";
}

/// <summary>
/// Rule: Event dates must be valid (start before end).
/// </summary>
public class EventMustHaveValidDatesRule : IBusinessRule
{
    private readonly EventDate _startDate;
    private readonly EventDate _endDate;

    public EventMustHaveValidDatesRule(EventDate startDate, EventDate endDate)
    {
        _startDate = startDate;
        _endDate = endDate;
    }

    public bool IsBroken() => _startDate.Value >= _endDate.Value;

    public string Message => "Event start date must be before end date.";
}