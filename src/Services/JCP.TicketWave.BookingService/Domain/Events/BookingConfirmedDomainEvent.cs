using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

public sealed record BookingConfirmedDomainEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId
) : DomainEvent;