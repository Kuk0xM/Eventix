namespace Eventix.EventCatalog.Application.Common.DTOs;

/// <summary>
/// Data Transfer Object for Event read operations.
/// Decoupled from the domain entity.
/// </summary>
public sealed record EventDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid VenueId { get; init; }
    public string Status { get; init; } = default!;
}