using JCP.TicketWave.Shared.Infrastructure.Events;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using JCP.TicketWave.Shared.Infrastructure.Messaging;

namespace JCP.TicketWave.PaymentService.Application.EventHandlers;

/// <summary>
/// Handler que procesa solicitudes de preparación de datos de pago desde BookingService
/// </summary>
public class PreparePaymentDataIntegrationEventHandler : IMessageHandler<PreparePaymentDataIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<PreparePaymentDataIntegrationEventHandler> _logger;

    public PreparePaymentDataIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<PreparePaymentDataIntegrationEventHandler> logger)
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task HandleAsync(PreparePaymentDataIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "💰 Processing PreparePaymentDataIntegrationEvent - BookingId: {BookingId}, Amount: {Amount}",
                integrationEvent.BookingId, integrationEvent.Amount);

            // Generar ID de pago único
            var paymentId = Guid.NewGuid();

            // Simular preparación de datos de pago
            var paymentUrl = $"https://payment.ticketwave.com/pay/{paymentId}";

            _logger.LogInformation(
                "💳 Payment data prepared - BookingId: {BookingId}, PaymentId: {PaymentId}",
                integrationEvent.BookingId, paymentId);

            // Publicar evento de datos de pago preparados
            var paymentDataPreparedEvent = new PaymentDataPreparedIntegrationEvent(
                BookingId: integrationEvent.BookingId,
                PaymentId: paymentId,
                Amount: integrationEvent.Amount,
                Currency: integrationEvent.Currency,
                ExpiresAt: integrationEvent.ExpiresAt,
                PaymentUrl: paymentUrl,
                PreparedAt: DateTime.UtcNow
            );

            await _integrationEventBus.PublishAsync(paymentDataPreparedEvent);

            _logger.LogInformation(
                "✅ Published PaymentDataPreparedIntegrationEvent - BookingId: {BookingId}, PaymentId: {PaymentId}",
                integrationEvent.BookingId, paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "💥 Error preparing payment data for BookingId: {BookingId}",
                integrationEvent.BookingId);
            throw;
        }
    }
}