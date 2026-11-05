using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.CancelEvent;

/// <summary>
/// Command to cancel an event.
/// </summary>
public sealed record CancelEventCommand : IRequest
{
    public Guid EventId { get; init; }
}