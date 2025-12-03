using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

public sealed record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int Quantity,
    decimal TotalAmount
) : DomainEvent;