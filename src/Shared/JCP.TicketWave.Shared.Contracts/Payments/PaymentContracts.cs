namespace JCP.TicketWave.Shared.Contracts.Payments;

public record PaymentProcessed(
    Guid PaymentId,
    string PaymentIntentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string CustomerId,
    DateTime ProcessedAt);

public record PaymentFailed(
    Guid PaymentId,
    string PaymentIntentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    string FailureReason,
    DateTime FailedAt);

public record RefundProcessed(
    Guid RefundId,
    string RefundIntentId,
    Guid PaymentId,
    decimal RefundAmount,
    string Reason,
    DateTime ProcessedAt);

public record RefundFailed(
    Guid RefundId,
    string RefundIntentId,
    Guid PaymentId,
    decimal RefundAmount,
    string FailureReason,
    DateTime FailedAt);