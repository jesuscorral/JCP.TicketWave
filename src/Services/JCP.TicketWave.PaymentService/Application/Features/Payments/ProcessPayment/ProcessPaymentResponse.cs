using JCP.TicketWave.PaymentService.Domain.Models;

namespace JCP.TicketWave.PaymentService.Application.Features.Payments.ProcessPayment;

public record ProcessPaymentResponse(
    Guid PaymentId,
    string PaymentIntentId,
    PaymentStatus Status,
    DateTime ProcessedAt,
    string? FailureReason);


