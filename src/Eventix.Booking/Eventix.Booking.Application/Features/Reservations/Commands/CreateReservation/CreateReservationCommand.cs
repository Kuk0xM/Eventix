using MediatR;

namespace Eventix.Booking.Application.Features.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(Guid EventId, Guid UserId, int Quantity) : IRequest<Guid>;