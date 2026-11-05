using Eventix.EventCatalog.Application.Common.Interfaces;
using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.PublishEvent;

/// <summary>
/// Handler for PublishEventCommand.
/// Orchestrates the publication of an existing event.
/// </summary>
public sealed class PublishEventHandler : IRequestHandler<PublishEventCommand>
{
    private readonly IEventRepository _eventRepository;

    public PublishEventHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        // 1. Load the event from the repository
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);

        if (@event is null)
            throw new InvalidOperationException($"Event with ID {request.EventId} not found.");

        // 2. Call the domain method (this adds a domain event internally)
        @event.Publish();

        // 3. Persist the changes
        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);
    }
}