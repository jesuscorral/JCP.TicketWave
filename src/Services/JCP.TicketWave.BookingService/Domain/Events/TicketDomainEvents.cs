using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

/// <summary>
/// Evento disparado cuando un ticket es reservado
/// </summary>
public sealed record TicketReservedDomainEvent(
    Guid TicketId,
    Guid BookingId,
    Guid EventId,
    string SeatNumber,
    decimal Price,
    DateTime ReservedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se libera una reserva de ticket
/// </summary>
public sealed record TicketReservationReleasedDomainEvent(
    Guid TicketId,
    Guid BookingId,
    Guid EventId,
    string SeatNumber,
    DateTime ReleasedAt,
    string Reason
) : DomainEvent;

/// <summary>
/// Evento disparado cuando un ticket es confirmado
/// </summary>
public sealed record TicketConfirmedDomainEvent(
    Guid TicketId,
    Guid BookingId,
    Guid EventId,
    string SeatNumber,
    decimal Price,
    DateTime ConfirmedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando una reserva expira
/// </summary>
public sealed record BookingExpiredDomainEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    string CustomerEmail,
    int Quantity,
    decimal TotalAmount,
    DateTime ExpirationTime
) : DomainEvent;