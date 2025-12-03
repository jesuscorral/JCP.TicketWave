using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.PaymentService.Domain.Events;

/// <summary>
/// Evento disparado cuando se inicia un pago
/// </summary>
public sealed record PaymentInitiatedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    Guid PaymentMethodId,
    string Provider,
    DateTime InitiatedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se procesa un pago exitosamente
/// </summary>
public sealed record PaymentProcessedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string TransactionId,
    string Provider,
    DateTime ProcessedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando falla un pago
/// </summary>
public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string FailureReason,
    string ErrorCode,
    DateTime FailedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se completa un pago
/// </summary>
public sealed record PaymentCompletedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string TransactionId,
    DateTime CompletedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se cancela un pago
/// </summary>
public sealed record PaymentCancelledDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Reason,
    DateTime CancelledAt
) : DomainEvent;