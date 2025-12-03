using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

public sealed record BookingCompletedDomainEvent(
    Guid BookingId,
    Guid EventId,
    decimal TotalAmount
) : DomainEvent;