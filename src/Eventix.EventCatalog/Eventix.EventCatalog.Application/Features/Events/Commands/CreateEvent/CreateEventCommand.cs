using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.CreateEvent;

/// <summary>
/// Command to create a new event.
/// Returns the ID of the created event.
/// </summary>
public sealed record CreateEventCommand : IRequest<Guid>
{
    public string Name { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid VenueId { get; init; }
}