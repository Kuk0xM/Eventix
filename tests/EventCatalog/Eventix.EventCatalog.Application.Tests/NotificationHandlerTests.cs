using Eventix.Chassis.Core.Messaging;
using Eventix.EventCatalog.Application.Features.Events.EventHandlers;
using Eventix.EventCatalog.Domain.Events;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Eventix.EventCatalog.Application.Tests;

public class NotificationHandlerTests
{
    [Fact]
    public async Task EventCreatedHandler_MapsAllFieldsToRichIntegrationEvent()
    {
        var outbox = new Mock<IOutboxPublisher>();
        EventCreatedIntegrationEvent? captured = null;
        outbox.Setup(x => x.EnqueueAsync(It.IsAny<EventCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()))
              .Callback<EventCreatedIntegrationEvent, CancellationToken>((e, _) => captured = e)
              .Returns(Task.CompletedTask);

        var handler = new EventCreatedEventHandler(outbox.Object, NullLogger<EventCreatedEventHandler>.Instance);
        var start = DateTime.UtcNow.AddDays(30);
        var end = DateTime.UtcNow.AddDays(31);
        var domainEvent = new EventCreatedEvent(Guid.NewGuid(), "Concert", EventDate.Create(start), EventDate.Create(end), Guid.NewGuid(), EventStatus.Draft);

        await handler.Handle(domainEvent, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.EventId.Should().Be(domainEvent.EventId);
        captured.Name.Should().Be("Concert");
        captured.Status.Should().Be("Draft");
        captured.StartDate.Should().Be(start);
        captured.EndDate.Should().Be(end);
    }

    [Fact]
    public async Task EventPublishedHandler_EnqueuesStatusChanged_Published()
    {
        var outbox = new Mock<IOutboxPublisher>();
        EventStatusChangedIntegrationEvent? captured = null;
        outbox.Setup(x => x.EnqueueAsync(It.IsAny<EventStatusChangedIntegrationEvent>(), It.IsAny<CancellationToken>()))
              .Callback<EventStatusChangedIntegrationEvent, CancellationToken>((e, _) => captured = e)
              .Returns(Task.CompletedTask);

        var handler = new EventPublishedEventHandler(outbox.Object, NullLogger<EventPublishedEventHandler>.Instance);
        var domainEvent = new EventPublishedEvent(Guid.NewGuid(), EventStatus.Published);

        await handler.Handle(domainEvent, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.NewStatus.Should().Be("Published");
        captured.EventId.Should().Be(domainEvent.EventId);
    }

    [Fact]
    public async Task EventCancelledHandler_EnqueuesStatusChanged_Cancelled()
    {
        var outbox = new Mock<IOutboxPublisher>();
        EventStatusChangedIntegrationEvent? captured = null;
        outbox.Setup(x => x.EnqueueAsync(It.IsAny<EventStatusChangedIntegrationEvent>(), It.IsAny<CancellationToken>()))
              .Callback<EventStatusChangedIntegrationEvent, CancellationToken>((e, _) => captured = e)
              .Returns(Task.CompletedTask);

        var handler = new EventCancelledEventHandler(outbox.Object, NullLogger<EventCancelledEventHandler>.Instance);
        var domainEvent = new EventCancelledEvent(Guid.NewGuid(), EventStatus.Cancelled);

        await handler.Handle(domainEvent, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.NewStatus.Should().Be("Cancelled");
    }
}