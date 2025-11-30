namespace JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;

public record ProcessPaymentCommand(
    Guid BookingId,
    decimal Amount,
    string Currency,
    string PaymentMethodId,
    string CustomerId,
    string IdempotencyKey);