using Eventix.Booking.Application.Common.Interfaces;
using Eventix.Booking.Application.Features.Events.EventHandlers;
using Eventix.Booking.Application.Features.Reservations.Commands.CreateReservation;
using Eventix.Booking.Application.Features.Reservations.EventHandlers;
using Eventix.Booking.Domain.Availability;
using Eventix.Booking.Domain.Reservations;
using Eventix.Booking.Domain.Reservations.Events;
using Eventix.Chassis.Core.Messaging;
using Eventix.SharedKernel.Contracts.Booking.Events;
using Eventix.SharedKernel.Contracts.Catalog.Events;
using Eventix.SharedKernel.Domain;
using FluentAssertions;
using Moq;

namespace Eventix.Booking.Application.Tests;

public class BookingHandlerTests
{
    [Fact]
    public async Task CreateReservation_WithStock_ReservesAndSaves()
    {
        var eventId = Guid.NewGuid();
        var availability = EventAvailability.Create(eventId, 100);
        var availabilityRepo = new Mock<IEventAvailabilityRepository>();
        availabilityRepo.Setup(x => x.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(availability);
        var reservationRepo = new Mock<IReservationRepository>();

        var handler = new CreateReservationHandler(availabilityRepo.Object, reservationRepo.Object);

        var id = await handler.Handle(new CreateReservationCommand(eventId, Guid.NewGuid(), 2), CancellationToken.None);

        id.Should().NotBeEmpty();
        availability.AvailableTickets.Should().Be(98);
        reservationRepo.Verify(x => x.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        reservationRepo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithoutStock_Throws()
    {
        var eventId = Guid.NewGuid();
        var availability = EventAvailability.Create(eventId, 5);
        var availabilityRepo = new Mock<IEventAvailabilityRepository>();
        availabilityRepo.Setup(x => x.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(availability);
        var reservationRepo = new Mock<IReservationRepository>();

        var handler = new CreateReservationHandler(availabilityRepo.Object, reservationRepo.Object);

        var act = () => handler.Handle(new CreateReservationCommand(eventId, Guid.NewGuid(), 10), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleValidationException>();
    }

    [Fact]
    public async Task EventCreatedIntegrationHandler_CreatesAvailabilityWith100Tickets()
    {
        var repo = new Mock<IEventAvailabilityRepository>();
        repo.Setup(x => x.GetByEventIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventAvailability?)null);
        EventAvailability? added = null;
        repo.Setup(x => x.AddAsync(It.IsAny<EventAvailability>(), It.IsAny<CancellationToken>()))
            .Callback<EventAvailability, CancellationToken>((a, _) => added = a)
            .Returns(Task.CompletedTask);

        var handler = new EventCreatedIntegrationEventHandler(repo.Object);
        var integrationEvent = new EventCreatedIntegrationEvent { EventId = Guid.NewGuid(), Name = "Concert" };

        await handler.Handle(integrationEvent, CancellationToken.None);

        added.Should().NotBeNull();
        added!.Id.Should().Be(integrationEvent.EventId);
        added.TotalTickets.Should().Be(100);
        added.AvailableTickets.Should().Be(100);
    }

    [Fact]
    public async Task ReservationCreatedHandler_EnqueuesIntegrationEvent()
    {
        var outbox = new Mock<IOutboxPublisher>();
        ReservationCreatedIntegrationEvent? captured = null;
        outbox.Setup(x => x.EnqueueAsync(It.IsAny<ReservationCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()))
              .Callback<ReservationCreatedIntegrationEvent, CancellationToken>((e, _) => captured = e)
              .Returns(Task.CompletedTask);

        var handler = new ReservationCreatedEventHandler(outbox.Object);
        var domainEvent = new ReservationCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3);

        await handler.Handle(domainEvent, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.ReservationId.Should().Be(domainEvent.ReservationId);
        captured.Quantity.Should().Be(3);
    }
}