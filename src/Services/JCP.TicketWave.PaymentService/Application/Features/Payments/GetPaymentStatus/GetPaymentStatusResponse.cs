using JCP.TicketWave.PaymentService.Domain.Models;

namespace JCP.TicketWave.PaymentService.Application.Features.Payments.GetPaymentStatus;

public record GetPaymentStatusResponse
{
    public required Guid PaymentId { get; init; }
    public required Guid BookingId { get; init; }
    public required decimal Amount { get; init; }
    public required PaymentStatus Status { get; init; }
    public string? TransactionId { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public string? FailureReason { get; init; }
}

