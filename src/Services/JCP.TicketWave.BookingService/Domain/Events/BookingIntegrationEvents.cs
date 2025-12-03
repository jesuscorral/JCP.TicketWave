using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Domain.Events;

/// <summary>
/// Evento de integración cuando un booking es creado
/// </summary>
public sealed record BookingCreatedIntegrationEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    string CustomerEmail,
    int Quantity,
    decimal TotalAmount,
    DateTime ExpiresAt
) : IntegrationEvent
{
    public override string EventType => "booking.created";
    public override string Source => "BookingService";
}

/// <summary>
/// Evento de integración cuando un booking es cancelado
/// </summary>
public sealed record BookingCancelledIntegrationEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    string Reason,
    DateTime CancelledAt
) : IntegrationEvent
{
    public override string EventType => "booking.cancelled";
    public override string Source => "BookingService";
}

/// <summary>
/// Evento de integración cuando un booking es completado
/// </summary>
public sealed record BookingCompletedIntegrationEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    decimal TotalAmount,
    DateTime CompletedAt
) : IntegrationEvent
{
    public override string EventType => "booking.completed";
    public override string Source => "BookingService";
}