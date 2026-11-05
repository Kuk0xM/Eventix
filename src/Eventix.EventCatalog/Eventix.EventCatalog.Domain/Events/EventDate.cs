namespace Eventix.EventCatalog.Domain.Events;

/// <summary>
/// Value Object representing the date of an event.
/// It is immutable and validates itself upon creation.
/// </summary>
public readonly record struct EventDate
{
    public DateTime Value { get; }

    private EventDate(DateTime value) => Value = value;

    /// <summary>
    /// Factory method to create a validated EventDate.
    /// </summary>
    public static EventDate Create(DateTime dateTime)
    {
        if (dateTime == default)
            throw new ArgumentException("Event date cannot be empty.", nameof(dateTime));

        return new EventDate(dateTime);
    }

    /// <summary>
    /// Checks if the date is in the future relative to a reference date.
    /// </summary>
    public bool IsFutureFrom(DateTime referenceDate) => Value > referenceDate;

    public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm");
}