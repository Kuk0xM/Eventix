namespace Eventix.EventCatalog.Domain.Venues;

/// <summary>
/// Value Object representing the location of a Venue.
/// </summary>
public sealed record VenueLocation
{
    public string City { get; }
    public string Country { get; }

    private VenueLocation(string city, string country)
    {
        City = city;
        Country = country;
    }

    /// <summary>
    /// Factory method to create a validated VenueLocation.
    /// </summary>
    public static VenueLocation Create(string city, string country)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        return new VenueLocation(city, country);
    }

    public override string ToString() => $"{City}, {Country}";
}