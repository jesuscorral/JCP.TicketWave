namespace JCP.TicketWave.PaymentService.Features.Refunds.ProcessRefund;

public record ProcessRefundCommand(
    Guid PaymentId,
    decimal? Amount, // null for full refund
    string Reason,
    string IdempotencyKey);