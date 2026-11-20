using Eventix.EventCatalog.Domain.Events;
using Eventix.SharedKernel.Domain;
using FluentAssertions;

namespace Eventix.EventCatalog.Domain.Tests;

public class EventTests
{
    private static Event NewEvent() => Event.Create(
        name: "Test Event",
        startDate: EventDate.Create(DateTime.UtcNow.AddDays(30)),
        endDate: EventDate.Create(DateTime.UtcNow.AddDays(31)),
        venueId: Guid.NewGuid());

    [Fact]
    public void Create_WithValidData_RaisesCreatedEvent()
    {
        var @event = NewEvent();

        @event.Status.Should().Be(EventStatus.Draft);
        @event.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EventCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyName_Throws()
    {
        var act = () => Event.Create(
            name: "",
            startDate: EventDate.Create(DateTime.UtcNow.AddDays(30)),
            endDate: EventDate.Create(DateTime.UtcNow.AddDays(31)),
            venueId: Guid.NewGuid());

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Publish_FromDraft_SetsPublishedAndRaisesEvent()
    {
        var @event = NewEvent();
        @event.ClearDomainEvents();

        @event.Publish();

        @event.Status.Should().Be(EventStatus.Published);
        @event.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EventPublishedEvent>();
    }

    [Fact]
    public void Publish_FromCancelled_Throws()
    {
        var @event = NewEvent();
        @event.Cancel();

        var act = () => @event.Publish();

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Cancel_FromPublished_SetsCancelledAndRaisesEvent()
    {
        var @event = NewEvent();
        @event.Publish();
        @event.ClearDomainEvents();

        @event.Cancel();

        @event.Status.Should().Be(EventStatus.Cancelled);
        @event.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<EventCancelledEvent>();
    }

    [Fact]
    public void Cancel_Twice_Throws()
    {
        var @event = NewEvent();
        @event.Cancel();

        var act = () => @event.Cancel();

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_WithEndBeforeStart_Throws()
    {
        var act = () => Event.Create(
            name: "Bad Range",
            startDate: EventDate.Create(DateTime.UtcNow.AddDays(31)),
            endDate: EventDate.Create(DateTime.UtcNow.AddDays(30)),
            venueId: Guid.NewGuid());

        act.Should().Throw<BusinessRuleValidationException>();
    }
}