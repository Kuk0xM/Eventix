using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.PublishEvent;

/// <summary>
/// Command to publish an event (change status from Draft to Published).
/// </summary>
public sealed record PublishEventCommand : IRequest
{
    public Guid EventId { get; init; }
}