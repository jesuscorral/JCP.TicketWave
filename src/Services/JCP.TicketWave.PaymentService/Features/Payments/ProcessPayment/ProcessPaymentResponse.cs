using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;

public record ProcessPaymentResponse(
    Guid PaymentId,
    string PaymentIntentId,
    PaymentStatus Status,
    DateTime ProcessedAt,
    string? FailureReason);

