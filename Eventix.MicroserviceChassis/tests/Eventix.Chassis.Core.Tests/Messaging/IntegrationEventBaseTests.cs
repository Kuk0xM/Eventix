using Eventix.Chassis.Core.Messaging;
using FluentAssertions;
using Xunit;

namespace Eventix.Chassis.Core.Tests.Messaging;

public class IntegrationEventBaseTests
{
    // Test event that inherits from our base
    private sealed record TestEvent(string Data) : IntegrationEventBase;

    [Fact]
    public void IntegrationEvent_ShouldAutoGenerateEventId()
    {
        // Act
        var @event = new TestEvent("test");

        // Assert
        @event.EventId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void IntegrationEvent_ShouldAutoSetOccurredAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var @event = new TestEvent("test");
        var after = DateTime.UtcNow;

        // Assert
        @event.OccurredAt.Should().BeOnOrAfter(before);
        @event.OccurredAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void IntegrationEvent_ShouldUseTypeNameAsEventType()
    {
        // Arrange & Act
        var @event = new TestEvent("test");

        // Assert
        @event.EventType.Should().Be("TestEvent");
    }

    [Fact]
    public void TwoIntegrationEvents_ShouldHaveDifferentIds()
    {
        // Arrange & Act
        var event1 = new TestEvent("test1");
        var event2 = new TestEvent("test2");

        // Assert
        event1.EventId.Should().NotBe(event2.EventId);
    }
}