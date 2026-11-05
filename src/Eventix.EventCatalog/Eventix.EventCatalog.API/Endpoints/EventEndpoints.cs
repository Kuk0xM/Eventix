using Eventix.EventCatalog.Application.Features.Events.Commands.CreateEvent;
using Eventix.EventCatalog.Application.Features.Events.Commands.PublishEvent;
using Eventix.EventCatalog.Application.Features.Events.Commands.CancelEvent;
using Eventix.EventCatalog.Application.Features.Events.Queries.GetEventById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Eventix.EventCatalog.Application.Common.DTOs;

namespace Eventix.EventCatalog.API.Endpoints;

/// <summary>
/// Extension methods for mapping Event-related endpoints.
/// </summary>
public static class EventEndpoints
{
    /// <summary>
    /// Maps all event endpoints to the application.
    /// </summary>
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup creates a group of endpoints with a common route prefix.
        // All endpoints inside will start with /api/events
        var group = app.MapGroup("/api/events")
            .WithTags("Events");  // Groups endpoints in Swagger UI

        // POST /api/events
        group.MapPost("/", CreateEvent)
            .WithName("CreateEvent")
            .WithSummary("Creates a new event")
            .Produces<Guid>(StatusCodes.Status201Created);

        // POST /api/events/{eventId}/publish
        group.MapPost("/{eventId:guid}/publish", PublishEvent)
            .WithName("PublishEvent")
            .WithSummary("Publishes an existing event")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/events/{eventId}/cancel
        group.MapPost("/{eventId:guid}/cancel", CancelEvent)
            .WithName("CancelEvent")
            .WithSummary("Cancels an existing event")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // GET /api/events/{eventId}
        group.MapGet("/{eventId:guid}", GetEventById)
            .WithName("GetEventById")
            .WithSummary("Gets an event by its ID")
            .Produces<EventDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    // =====================================================
    // HANDLER METHODS (one per endpoint)
    // =====================================================

    /// <summary>
    /// Creates a new event.
    /// </summary>
    private static async Task<IResult> CreateEvent(
        [FromBody] CreateEventCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var eventId = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/events/{eventId}", new { Id = eventId });
    }

    /// <summary>
    /// Publishes an existing event.
    /// </summary>
    private static async Task<IResult> PublishEvent(
        Guid eventId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new PublishEventCommand { EventId = eventId };
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    /// <summary>
    /// Cancels an existing event.
    /// </summary>
    private static async Task<IResult> CancelEvent(
        Guid eventId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CancelEventCommand { EventId = eventId };
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    /// <summary>
    /// Gets an event by its ID.
    /// </summary>
    private static async Task<IResult> GetEventById(
        Guid eventId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetEventByIdQuery { EventId = eventId };
        var result = await sender.Send(query, cancellationToken);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }
}