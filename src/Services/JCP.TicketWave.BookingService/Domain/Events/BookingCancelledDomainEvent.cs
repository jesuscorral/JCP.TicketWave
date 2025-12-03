using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

public sealed record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid EventId,
    string Reason
) : DomainEvent;