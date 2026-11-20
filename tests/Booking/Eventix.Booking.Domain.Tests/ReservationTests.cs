using Eventix.Booking.Domain.Availability;
using Eventix.Booking.Domain.Reservations;
using Eventix.Booking.Domain.Reservations.Events;
using Eventix.SharedKernel.Domain;
using FluentAssertions;

namespace Eventix.Booking.Domain.Tests;

public class ReservationTests
{
    [Fact]
    public void Create_WithValidData_RaisesReservationCreatedEvent()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2);

        reservation.Status.Should().Be(ReservationStatus.Pending);
        reservation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ReservationCreatedEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveQuantity_Throws(int quantity)
    {
        var act = () => Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), quantity);

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Cancel_PendingReservation_RaisesCancelledEvent()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2);
        reservation.ClearDomainEvents();

        reservation.Cancel();

        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        reservation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ReservationCancelledEvent>();
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2);
        reservation.Cancel();

        var act = () => reservation.Cancel();

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Confirm_PendingReservation_SetsConfirmed()
    {
        var reservation = Reservation.Create(Guid.NewGuid(), Guid.NewGuid(), 2);

        reservation.Confirm();

        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }
}

public class EventAvailabilityTests
{
    [Fact]
    public void Reserve_WithEnoughStock_DecrementsAvailable()
    {
        var availability = EventAvailability.Create(Guid.NewGuid(), 100);

        availability.Reserve(10);

        availability.AvailableTickets.Should().Be(90);
        availability.TotalTickets.Should().Be(100);
    }

    [Fact]
    public void Reserve_WithoutEnoughStock_Throws()
    {
        var availability = EventAvailability.Create(Guid.NewGuid(), 5);

        var act = () => availability.Reserve(10);

        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Release_NeverExceedsTotal()
    {
        var availability = EventAvailability.Create(Guid.NewGuid(), 100);
        availability.Reserve(10);

        availability.Release(50);

        availability.AvailableTickets.Should().Be(100);
    }
}