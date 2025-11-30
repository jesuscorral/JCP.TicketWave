namespace JCP.TicketWave.PaymentService.Features.Payments.GetPaymentStatus;

public record GetPaymentStatusResponse(
    Guid PaymentId,
    string PaymentIntentId,
    Guid BookingId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTime ProcessedAt,
    string? FailureReason);
