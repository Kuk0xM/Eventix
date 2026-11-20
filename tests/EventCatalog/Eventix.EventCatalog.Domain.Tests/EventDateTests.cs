using Eventix.EventCatalog.Domain.Events;
using FluentAssertions;

namespace Eventix.EventCatalog.Domain.Tests;

public class EventDateTests
{
    [Fact]
    public void Create_WithValidDate_PreservesValue()
    {
        var start = DateTime.UtcNow.AddDays(30);

        var date = EventDate.Create(start);

        date.Value.Should().Be(start);
    }
}