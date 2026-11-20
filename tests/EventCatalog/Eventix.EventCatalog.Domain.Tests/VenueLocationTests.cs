using Eventix.EventCatalog.Domain.Venues;
using FluentAssertions;

namespace Eventix.EventCatalog.Domain.Tests;

public class VenueLocationTests
{
    [Fact]
    public void Create_WithValidData_PreservesValues()
    {
        var location = VenueLocation.Create("Buenos Aires", "Argentina");

        location.City.Should().Be("Buenos Aires");
        location.Country.Should().Be("Argentina");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCity_ThrowsArgumentException(string city)
    {
        var act = () => VenueLocation.Create(city, "Argentina");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCountry_ThrowsArgumentException(string country)
    {
        var act = () => VenueLocation.Create("Buenos Aires", country);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToString_FormatsAsCityCommaCountry()
    {
        var location = VenueLocation.Create("Buenos Aires", "Argentina");

        location.ToString().Should().Be("Buenos Aires, Argentina");
    }
}