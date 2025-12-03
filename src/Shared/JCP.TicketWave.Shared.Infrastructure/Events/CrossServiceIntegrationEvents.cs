using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.Shared.Infrastructure.Events;

/// <summary>
/// Integration event para solicitar actualización de inventario en CatalogService
/// Publicado por BookingService cuando se crea un booking
/// </summary>
public sealed record UpdateEventInventoryIntegrationEvent(
    Guid EventId,
    Guid BookingId,
    int QuantityReduced,
    DateTime RequestedAt
) : IntegrationEvent
{
    public override string EventType => "catalog.inventory.update.requested";
    public override string Source => "BookingService";
}

/// <summary>
/// Integration event para solicitar envío de notificación
/// Publicado por BookingService cuando se crea un booking
/// </summary>
public sealed record SendBookingNotificationIntegrationEvent(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    int Quantity,
    decimal TotalAmount,
    string NotificationType,
    DateTime RequestedAt
) : IntegrationEvent
{
    public override string EventType => "notification.booking.send.requested";
    public override string Source => "BookingService";
}

/// <summary>
/// Integration event para solicitar preparación de datos de pago
/// Publicado por BookingService cuando se crea un booking que requiere pago
/// </summary>
public sealed record PreparePaymentDataIntegrationEvent(
    Guid BookingId,
    decimal Amount,
    string Currency,
    Guid UserId,
    DateTime ExpiresAt,
    DateTime RequestedAt
) : IntegrationEvent
{
    public override string EventType => "payment.preparation.requested";
    public override string Source => "BookingService";
}

/// <summary>
/// Integration event para notificar actualización de inventario completada
/// Publicado por CatalogService después de actualizar inventario
/// </summary>
public sealed record InventoryUpdatedIntegrationEvent(
    Guid EventId,
    Guid BookingId,
    int PreviousAvailableTickets,
    int CurrentAvailableTickets,
    string UpdateType,
    DateTime UpdatedAt
) : IntegrationEvent
{
    public override string EventType => "catalog.inventory.updated";
    public override string Source => "CatalogService";
}

/// <summary>
/// Integration event para notificar que una notificación fue enviada
/// Publicado por NotificationService después de enviar una notificación
/// </summary>
public sealed record NotificationSentIntegrationEvent(
    Guid BookingId,
    Guid UserId,
    string NotificationType,
    string Channel, // email, sms, push
    bool Success,
    string? ErrorMessage,
    DateTime SentAt
) : IntegrationEvent
{
    public override string EventType => "notification.sent";
    public override string Source => "NotificationService";
}

/// <summary>
/// Integration event para notificar que los datos de pago fueron preparados
/// Publicado por PaymentService después de preparar una transacción
/// </summary>
public sealed record PaymentDataPreparedIntegrationEvent(
    Guid BookingId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    DateTime ExpiresAt,
    string PaymentUrl,
    DateTime PreparedAt
) : IntegrationEvent
{
    public override string EventType => "payment.data.prepared";
    public override string Source => "PaymentService";
}