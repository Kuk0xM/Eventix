using Eventix.EventCatalog.Domain.Venues;
using FluentAssertions;

namespace Eventix.EventCatalog.Domain.Tests;

public class VenueTests
{
    private static VenueLocation ValidLocation() => VenueLocation.Create("Buenos Aires", "Argentina");

    [Fact]
    public void Ctor_WithNonPositiveCapacity_ThrowsArgumentException()
    {
        var act = () => new Venue(Guid.NewGuid(), "Stadium", 0, ValidLocation());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => new Venue(Guid.NewGuid(), "", 100, ValidLocation());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateCapacity_WithValidValue_Updates()
    {
        var venue = new Venue(Guid.NewGuid(), "Stadium", 100, ValidLocation());

        venue.UpdateCapacity(200);

        venue.Capacity.Should().Be(200);
    }

    [Fact]
    public void UpdateCapacity_WithNonPositive_ThrowsArgumentException()
    {
        var venue = new Venue(Guid.NewGuid(), "Stadium", 100, ValidLocation());

        var act = () => venue.UpdateCapacity(-5);

        act.Should().Throw<ArgumentException>();
    }
}