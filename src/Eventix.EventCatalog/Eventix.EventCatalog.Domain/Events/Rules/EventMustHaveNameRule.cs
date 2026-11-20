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