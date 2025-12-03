using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.CatalogService.Domain.Events;

/// <summary>
/// Evento disparado cuando se crea un nuevo venue
/// </summary>
public sealed record VenueCreatedDomainEvent(
    Guid VenueId,
    string Name,
    string Address,
    string City,
    string Country,
    int Capacity,
    DateTime CreatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se actualiza un venue
/// </summary>
public sealed record VenueUpdatedDomainEvent(
    Guid VenueId,
    string Name,
    string Address,
    string City,
    string Country,
    int Capacity,
    DateTime UpdatedAt
) : DomainEvent;

/// <summary>
/// Evento de integración cuando se crea un evento (para otros servicios)
/// </summary>
public sealed record EventCreatedIntegrationEvent(
    Guid EventId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid VenueId,
    string VenueName,
    Guid CategoryId,
    string CategoryName,
    decimal BasePrice,
    string Currency,
    int TotalCapacity
) : IntegrationEvent
{
    public override string EventType => "event.created";
    public override string Source => "CatalogService";
}

/// <summary>
/// Evento de integración cuando se cancela un evento
/// </summary>
public sealed record EventCancelledIntegrationEvent(
    Guid EventId,
    string Name,
    DateTime StartDate,
    string Reason,
    DateTime CancelledAt
) : IntegrationEvent
{
    public override string EventType => "event.cancelled";
    public override string Source => "CatalogService";
}