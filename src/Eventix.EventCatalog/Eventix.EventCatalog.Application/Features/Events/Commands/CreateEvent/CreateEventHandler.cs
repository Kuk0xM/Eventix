using System.Diagnostics;
using Eventix.EventCatalog.Application.Common.Interfaces;
using Eventix.EventCatalog.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Eventix.EventCatalog.Application.Features.Events.Commands.CreateEvent;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<CreateEventHandler> _logger;

    private static readonly ActivitySource ActivitySource = new("Eventix.EventCatalog");

    public CreateEventHandler(
        IEventRepository eventRepository,
        ILogger<CreateEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("CreateEvent");

        activity?.SetTag("event.name", request.Name);
        activity?.SetTag("event.venue_id", request.VenueId.ToString());

        _logger.LogInformation(
            "Creating event {EventName} for venue {VenueId}",
            request.Name,
            request.VenueId);

        try
        {
            var @event = Event.Create(
                name: request.Name,
                startDate: EventDate.Create(request.StartDate),
                endDate: EventDate.Create(request.EndDate),
                venueId: request.VenueId
            );

            activity?.SetTag("event.id", @event.Id.ToString());

            await _eventRepository.AddAsync(@event, cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Event {EventId} created successfully. Name: {EventName}",
                @event.Id,
                @event.Name);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return @event.Id;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            activity?.AddException(ex);

            _logger.LogError(ex, "Failed to create event {EventName}", request.Name);
            throw;
        }
    }
}