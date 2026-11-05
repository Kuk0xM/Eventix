using Eventix.EventCatalog.Application.Common.DTOs;
using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Queries.GetEventById;

/// <summary>
/// Query to get an event by its ID.
/// Returns a DTO, not the domain entity.
/// </summary>
public sealed record GetEventByIdQuery : IRequest<EventDto?>
{
    public Guid EventId { get; init; }
}