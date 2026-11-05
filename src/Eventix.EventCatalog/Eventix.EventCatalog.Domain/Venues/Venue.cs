using Eventix.SharedKernel.Domain;

namespace Eventix.EventCatalog.Domain.Venues;

/// <summary>
/// Entity representing a place where events are held (stadiums, theaters, etc.).
/// </summary>
public class Venue : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public VenueLocation Location { get; private set; }

    // Private constructor for EF Core (Hydration)
    private Venue() { }

    public Venue(Guid id, string name, int capacity, VenueLocation location)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Venue name cannot be empty.", nameof(name));

        if (capacity <= 0)
            throw new ArgumentException("Venue capacity must be greater than zero.", nameof(capacity));

        Name = name;
        Capacity = capacity;
        Location = location;
    }

    /// <summary>
    /// Updates the venue capacity.
    /// </summary>
    public void UpdateCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
            throw new ArgumentException("Capacity must be greater than zero.");

        Capacity = newCapacity;
    }
}