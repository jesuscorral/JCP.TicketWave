using JCP.TicketWave.Shared.Infrastructure.Events;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using JCP.TicketWave.Shared.Infrastructure.Messaging;

namespace JCP.TicketWave.NotificationService.Application.EventHandlers;

/// <summary>
/// Handler que procesa solicitudes de envío de notificaciones desde BookingService
/// </summary>
public class SendBookingNotificationIntegrationEventHandler : IMessageHandler<SendBookingNotificationIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<SendBookingNotificationIntegrationEventHandler> _logger;

    public SendBookingNotificationIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        ILogger<SendBookingNotificationIntegrationEventHandler> logger)
    {
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task HandleAsync(SendBookingNotificationIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "📧 Processing SendBookingNotificationIntegrationEvent - BookingId: {BookingId}, Type: {Type}",
                integrationEvent.BookingId, integrationEvent.NotificationType);

            // Simular envío de notificación
            var notificationSuccess = true;
            var channel = "email"; // Por defecto email
            string? errorMessage = null;

            // Simular posible fallo del 5%
            if (Random.Shared.NextDouble() < 0.05)
            {
                notificationSuccess = false;
                errorMessage = "SMTP server temporarily unavailable";
            }

            if (notificationSuccess)
            {
                _logger.LogInformation(
                    "✅ Notification sent successfully - BookingId: {BookingId}, Channel: {Channel}",
                    integrationEvent.BookingId, channel);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Notification failed - BookingId: {BookingId}, Error: {Error}",
                    integrationEvent.BookingId, errorMessage);
            }

            // Publicar evento de notificación enviada
            var notificationSentEvent = new NotificationSentIntegrationEvent(
                BookingId: integrationEvent.BookingId,
                UserId: integrationEvent.UserId,
                NotificationType: integrationEvent.NotificationType,
                Channel: channel,
                Success: notificationSuccess,
                ErrorMessage: errorMessage,
                SentAt: DateTime.UtcNow
            );

            await _integrationEventBus.PublishAsync(notificationSentEvent);

            _logger.LogInformation(
                "📤 Published NotificationSentIntegrationEvent - BookingId: {BookingId}, Success: {Success}",
                integrationEvent.BookingId, notificationSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "💥 Error sending notification for BookingId: {BookingId}",
                integrationEvent.BookingId);
            throw;
        }
    }
}