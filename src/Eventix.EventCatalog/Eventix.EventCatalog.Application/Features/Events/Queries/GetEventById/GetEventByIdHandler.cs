using Eventix.EventCatalog.Application.Common.DTOs;
using Eventix.EventCatalog.Application.Common.Interfaces;
using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Queries.GetEventById;

/// <summary>
/// Handler for GetEventByIdQuery.
/// Retrieves event data and maps it to a DTO.
/// </summary>
public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Load the event from the repository
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);

        if (@event is null)
            return null;

        // 2. Map domain entity to DTO
        return new EventDto
        {
            Id = @event.Id,
            Name = @event.Name,
            StartDate = @event.StartDate.Value,
            EndDate = @event.EndDate.Value,
            VenueId = @event.VenueId,
            Status = @event.Status.ToString()
        };
    }
}