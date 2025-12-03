using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.CatalogService.Domain.Events;

/// <summary>
/// Evento disparado cuando se crea una nueva categoría
/// </summary>
public sealed record CategoryCreatedDomainEvent(
    Guid CategoryId,
    string Name,
    string Description,
    string Color,
    DateTime CreatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se actualiza una categoría
/// </summary>
public sealed record CategoryUpdatedDomainEvent(
    Guid CategoryId,
    string Name,
    string Description,
    string Color,
    DateTime UpdatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se desactiva una categoría
/// </summary>
public sealed record CategoryDeactivatedDomainEvent(
    Guid CategoryId,
    string Name,
    DateTime DeactivatedAt,
    string Reason
) : DomainEvent;