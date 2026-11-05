using Eventix.EventCatalog.Application.Common.Interfaces;
using MediatR;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.CancelEvent;

/// <summary>
/// Handler for CancelEventCommand.
/// Orchestrates the cancellation of an existing event.
/// </summary>
public sealed class CancelEventHandler : IRequestHandler<CancelEventCommand>
{
    private readonly IEventRepository _eventRepository;

    public CancelEventHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task Handle(CancelEventCommand request, CancellationToken cancellationToken)
    {
        // 1. Load the event from the repository
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);

        if (@event is null)
            throw new InvalidOperationException($"Event with ID {request.EventId} not found.");

        // 2. Call the domain method (this adds a domain event internally)
        @event.Cancel();

        // 3. Persist the changes
        await _eventRepository.UpdateAsync(@event, cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);
    }
}