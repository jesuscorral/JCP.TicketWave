using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.CatalogService.Domain.Events;

/// <summary>
/// Evento disparado cuando se crea un nuevo evento
/// </summary>
public sealed record EventCreatedDomainEvent(
    Guid EventId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid VenueId,
    Guid CategoryId,
    decimal BasePrice,
    string Currency,
    int TotalCapacity
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se actualiza un evento
/// </summary>
public sealed record EventUpdatedDomainEvent(
    Guid EventId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Currency,
    decimal BasePrice,
    DateTime UpdatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se publica un evento
/// </summary>
public sealed record EventPublishedDomainEvent(
    Guid EventId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    DateTime PublishedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se cancela un evento
/// </summary>
public sealed record EventCancelledDomainEvent(
    Guid EventId,
    string Name,
    DateTime StartDate,
    string Reason,
    DateTime CancelledAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se agota la capacidad de un evento
/// </summary>
public sealed record EventSoldOutDomainEvent(
    Guid EventId,
    string Name,
    DateTime StartDate,
    int TotalCapacity,
    DateTime SoldOutAt
) : DomainEvent;