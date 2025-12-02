using JCP.TicketWave.PaymentService.Domain.Models;

namespace JCP.TicketWave.PaymentService.Application.Features.Refunds.ProcessRefund;

public record ProcessRefundResponse(
    Guid RefundId,
    string RefundIntentId,
    decimal RefundAmount,
    RefundStatus Status,
    DateTime ProcessedAt,
    string? FailureReason);
