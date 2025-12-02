using JCP.TicketWave.PaymentService.Domain.Interfaces;

namespace JCP.TicketWave.PaymentService.Features.Payments.GetPaymentStatus;

public class GetPaymentStatusHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentStatusHandler> _logger;

    public GetPaymentStatusHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentStatusHandler> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetPaymentStatusResponse?> Handle(GetPaymentStatusQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting payment status for payment {PaymentId}", query.PaymentId);

            var payment = await _paymentRepository.GetByIdAsync(query.PaymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", query.PaymentId);
                return null;
            }

            return new GetPaymentStatusResponse
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Status = payment.Status,
                TransactionId = payment.ExternalPaymentId,
                ProcessedAt = payment.ProcessedAt,
                CreatedAt = payment.CreatedAt,
                FailureReason = payment.FailureReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for payment {PaymentId}", query.PaymentId);
            throw;
        }
    }
}