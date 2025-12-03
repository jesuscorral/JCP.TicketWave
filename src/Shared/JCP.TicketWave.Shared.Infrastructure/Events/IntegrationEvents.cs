using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.Shared.Infrastructure.Events;

/// <summary>
/// Eventos de integración para BookingService
/// </summary>
/// 
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
/// Evento de integración cuando un booking se confirma (para otros servicios)
/// </summary>
public sealed record BookingConfirmedIntegrationEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    string CustomerEmail,
    int Quantity,
    decimal TotalAmount,
    DateTime ConfirmedAt
) : IntegrationEvent
{
    public override string EventType => "booking.confirmed";
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

/// <summary>
/// Eventos de integración para PaymentService
/// </summary>

/// <summary>
/// Evento de integración cuando un pago se completa
/// </summary>
public sealed record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string TransactionId,
    DateTime CompletedAt
) : IntegrationEvent
{
    public override string EventType => "payment.completed";
    public override string Source => "PaymentService";
}

/// <summary>
/// Evento de integración cuando un pago falla
/// </summary>
public sealed record PaymentFailedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string FailureReason,
    DateTime FailedAt
) : IntegrationEvent
{
    public override string EventType => "payment.failed";
    public override string Source => "PaymentService";
}

/// <summary>
/// Eventos de integración para CatalogService
/// </summary>

/// <summary>
/// Evento de integración cuando se crea un evento
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