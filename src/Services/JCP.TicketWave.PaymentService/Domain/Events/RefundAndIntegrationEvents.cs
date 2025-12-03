using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.PaymentService.Domain.Events;

/// <summary>
/// Evento disparado cuando se inicia un reembolso
/// </summary>
public sealed record RefundInitiatedDomainEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string Reason,
    DateTime InitiatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se procesa un reembolso
/// </summary>
public sealed record RefundProcessedDomainEvent(
    Guid RefundId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string TransactionId,
    DateTime ProcessedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando falla un reembolso
/// </summary>
public sealed record RefundFailedDomainEvent(
    Guid RefundId,
    Guid PaymentId,
    decimal Amount,
    string Currency,
    string FailureReason,
    DateTime FailedAt
) : DomainEvent;

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
/// Evento de integración cuando un reembolso se procesa
/// </summary>
public sealed record RefundProcessedIntegrationEvent(
    Guid RefundId,
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string TransactionId,
    DateTime ProcessedAt
) : IntegrationEvent
{
    public override string EventType => "refund.processed";
    public override string Source => "PaymentService";
}