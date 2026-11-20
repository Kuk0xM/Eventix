using Eventix.Booking.Domain.Availability;

namespace Eventix.Booking.Application.Common.Interfaces;

public interface IEventAvailabilityRepository
{
    Task AddAsync(EventAvailability availability, CancellationToken cancellationToken = default);
    Task<EventAvailability?> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}