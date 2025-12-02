using JCP.TicketWave.PaymentService.Domain.Entities;
using JCP.TicketWave.PaymentService.Domain.Interfaces;

namespace JCP.TicketWave.PaymentService.Features.Payments.ProcessPayment;

public class ProcessPaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly ILogger<ProcessPaymentHandler> _logger;

    public ProcessPaymentHandler(
        IPaymentRepository paymentRepository,
        IPaymentMethodRepository paymentMethodRepository,
        ILogger<ProcessPaymentHandler> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _paymentMethodRepository = paymentMethodRepository ?? throw new ArgumentNullException(nameof(paymentMethodRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessPaymentResponse> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing payment for booking {BookingId} with amount {Amount}", 
                command.BookingId, command.Amount);

            // Validate payment method exists
            var paymentMethod = await _paymentMethodRepository.GetByExternalIdAsync(command.PaymentMethodId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found", command.PaymentMethodId);
                return new ProcessPaymentResponse(
                    PaymentId: Guid.NewGuid(),
                    PaymentIntentId: string.Empty,
                    Status: PaymentStatus.Failed,
                    ProcessedAt: DateTime.UtcNow,
                    FailureReason: "Payment method not found"
                );
            }

            // Create payment entity
            var payment = Payment.Create(
                command.BookingId,
                command.Amount,
                "USD", // Default currency
                paymentMethod);

            // Process payment (this would integrate with payment gateway)
            var processingResult = await ProcessWithPaymentGateway(payment, paymentMethod, cancellationToken);
            
            if (processingResult.IsSuccess)
            {
                if (processingResult.TransactionId != null)
                {
                    payment.MarkAsProcessing(processingResult.TransactionId);
                }
                payment.MarkAsSucceeded();
                _logger.LogInformation("Payment {PaymentId} completed successfully", payment.Id);
            }
            else
            {
                payment.MarkAsFailed(processingResult.FailureReason ?? "Unknown payment gateway error");
                _logger.LogWarning("Payment {PaymentId} failed: {Reason}", payment.Id, processingResult.FailureReason);
            }

            // Save payment
            await _paymentRepository.CreateAsync(payment);

            return new ProcessPaymentResponse(
                PaymentId: payment.Id,
                PaymentIntentId: payment.ExternalPaymentId ?? $"pi_{payment.Id:N}",
                Status: payment.Status,
                ProcessedAt: DateTime.UtcNow,
                FailureReason: payment.Status == PaymentStatus.Failed ? payment.FailureReason : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for booking {BookingId}", command.BookingId);
            throw;
        }
    }

    private async Task<PaymentGatewayResult> ProcessWithPaymentGateway(Payment payment, PaymentMethod paymentMethod, CancellationToken cancellationToken)
    {
        // Simulate payment gateway integration
        await Task.Delay(100, cancellationToken); // Simulate network call
        
        // For demonstration, we'll simulate a 90% success rate
        var isSuccess = Random.Shared.NextDouble() > 0.1;
        
        if (isSuccess)
        {
            return new PaymentGatewayResult
            {
                IsSuccess = true,
                TransactionId = $"TXN_{Guid.NewGuid():N}"
            };
        }
        else
        {
            return new PaymentGatewayResult
            {
                IsSuccess = false,
                FailureReason = "Insufficient funds"
            };
        }
    }

    private class PaymentGatewayResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
    }
}