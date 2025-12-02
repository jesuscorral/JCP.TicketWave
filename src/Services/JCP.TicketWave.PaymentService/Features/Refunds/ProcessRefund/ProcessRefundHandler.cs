using JCP.TicketWave.PaymentService.Domain.Entities;
using JCP.TicketWave.PaymentService.Domain.Interfaces;

namespace JCP.TicketWave.PaymentService.Features.Refunds.ProcessRefund;

public class ProcessRefundHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IRefundRepository _refundRepository;
    private readonly ILogger<ProcessRefundHandler> _logger;

    public ProcessRefundHandler(
        IPaymentRepository paymentRepository,
        IRefundRepository refundRepository,
        ILogger<ProcessRefundHandler> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _refundRepository = refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProcessRefundResponse> Handle(ProcessRefundCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Processing refund for payment {PaymentId} with amount {Amount}", 
                command.PaymentId, command.Amount);

            // Get the original payment
            var payment = await _paymentRepository.GetByIdAsync(command.PaymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for refund", command.PaymentId);
                return new ProcessRefundResponse(
                    RefundId: Guid.NewGuid(),
                    RefundIntentId: string.Empty,
                    RefundAmount: command.Amount ?? 0,
                    Status: RefundStatus.Failed,
                    ProcessedAt: DateTime.UtcNow,
                    FailureReason: "Original payment not found"
                );
            }

            // Validate payment is refundable
            if (payment.Status != PaymentStatus.Succeeded)
            {
                _logger.LogWarning("Payment {PaymentId} is not in a refundable status: {Status}", 
                    command.PaymentId, payment.Status);
                return new ProcessRefundResponse(
                    RefundId: Guid.NewGuid(),
                    RefundIntentId: string.Empty,
                    RefundAmount: command.Amount ?? 0,
                    Status: RefundStatus.Failed,
                    ProcessedAt: DateTime.UtcNow,
                    FailureReason: "Payment is not in a refundable status"
                );
            }

            // Determine refund amount
            var refundAmount = command.Amount ?? payment.Amount;

            // Validate refund amount
            if (refundAmount <= 0)
            {
                _logger.LogWarning("Invalid refund amount {Amount} for payment {PaymentId}", 
                    refundAmount, command.PaymentId);
                return new ProcessRefundResponse(
                    RefundId: Guid.NewGuid(),
                    RefundIntentId: string.Empty,
                    RefundAmount: refundAmount,
                    Status: RefundStatus.Failed,
                    ProcessedAt: DateTime.UtcNow,
                    FailureReason: "Refund amount must be greater than zero"
                );
            }

            // Check if refund amount doesn't exceed remaining refundable amount
            var existingRefunds = await _refundRepository.GetByPaymentIdAsync(command.PaymentId);
            var totalRefunded = existingRefunds.Where(r => r.Status == RefundStatus.Succeeded)
                                              .Sum(r => r.Amount);
            
            if (refundAmount > payment.Amount - totalRefunded)
            {
                _logger.LogWarning("Refund amount {RefundAmount} exceeds available amount {AvailableAmount} for payment {PaymentId}", 
                    refundAmount, payment.Amount - totalRefunded, command.PaymentId);
                return new ProcessRefundResponse(
                    RefundId: Guid.NewGuid(),
                    RefundIntentId: string.Empty,
                    RefundAmount: refundAmount,
                    Status: RefundStatus.Failed,
                    ProcessedAt: DateTime.UtcNow,
                    FailureReason: "Refund amount exceeds available refundable amount"
                );
            }

            // Create refund entity
            var refund = Refund.Create(
                command.PaymentId,
                refundAmount,
                "USD", // Default currency - could be derived from payment
                command.Reason ?? "Refund requested");

            // Process refund with payment gateway
            var processingResult = await ProcessWithPaymentGateway(refund, payment, cancellationToken);
            
            if (processingResult.IsSuccess)
            {
                if (processingResult.TransactionId != null)
                {
                    refund.MarkAsProcessing(processingResult.TransactionId);
                }
                refund.MarkAsSucceeded();
                _logger.LogInformation("Refund {RefundId} completed successfully", refund.Id);
            }
            else
            {
                refund.MarkAsFailed(processingResult.FailureReason ?? "Unknown payment gateway error");
                _logger.LogWarning("Refund {RefundId} failed: {Reason}", refund.Id, processingResult.FailureReason);
            }

            // Save refund
            await _refundRepository.CreateAsync(refund);

            return new ProcessRefundResponse(
                RefundId: refund.Id,
                RefundIntentId: refund.ExternalRefundId ?? $"re_{refund.Id:N}",
                RefundAmount: refund.Amount,
                Status: refund.Status,
                ProcessedAt: DateTime.UtcNow,
                FailureReason: refund.Status == RefundStatus.Failed ? refund.FailureReason : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", command.PaymentId);
            throw;
        }
    }

    private async Task<RefundGatewayResult> ProcessWithPaymentGateway(Refund refund, Payment payment, CancellationToken cancellationToken)
    {
        // Simulate payment gateway refund integration
        await Task.Delay(100, cancellationToken); // Simulate network call
        
        // For demonstration, we'll simulate a 95% success rate
        var isSuccess = Random.Shared.NextDouble() > 0.05;
        
        if (isSuccess)
        {
            return new RefundGatewayResult
            {
                IsSuccess = true,
                TransactionId = $"REF_{Guid.NewGuid():N}"
            };
        }
        else
        {
            var failureReasons = new[]
            {
                "Payment method does not support refunds",
                "Refund period expired",
                "Insufficient balance in merchant account"
            };
            
            return new RefundGatewayResult
            {
                IsSuccess = false,
                FailureReason = failureReasons[Random.Shared.Next(failureReasons.Length)]
            };
        }
    }

    private class RefundGatewayResult
    {
        public bool IsSuccess { get; set; }
        public string? TransactionId { get; set; }
        public string? FailureReason { get; set; }
    }
}