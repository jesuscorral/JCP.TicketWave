namespace JCP.TicketWave.PaymentService.Features.Refunds.ProcessRefund;

public record ProcessRefundResponse(
    Guid RefundId,
    string RefundIntentId,
    decimal RefundAmount,
    RefundStatus Status,
    DateTime ProcessedAt,
    string? FailureReason);

public enum RefundStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Cancelled
}