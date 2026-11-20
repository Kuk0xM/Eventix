using Eventix.Booking.Application.Common.Interfaces;
using Eventix.Booking.Domain.Reservations;
using MediatR;

namespace Eventix.Booking.Application.Features.Reservations.Commands.CreateReservation;

public sealed class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Guid>
{
    private readonly IEventAvailabilityRepository _availabilityRepository;
    private readonly IReservationRepository _reservationRepository;

    public CreateReservationHandler(
        IEventAvailabilityRepository availabilityRepository,
        IReservationRepository reservationRepository)
    {
        _availabilityRepository = availabilityRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var availability = await _availabilityRepository.GetByEventIdAsync(request.EventId, cancellationToken)
            ?? throw new InvalidOperationException($"No availability found for event {request.EventId}.");

        availability.Reserve(request.Quantity);

        var reservation = Reservation.Create(request.EventId, request.UserId, request.Quantity);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}