using JCP.TicketWave.BookingService.Domain.Events;
using JCP.TicketWave.Shared.Infrastructure.Events;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.BookingService.Application.EventHandlers;

/// <summary>
/// Handler que responde al domain event BookingCreatedDomainEvent
/// Publica integration events para comunicar con otros servicios
/// </summary>
public class BookingCreatedDomainEventHandler : IDomainEventHandler<BookingCreatedDomainEvent>
{
    private readonly ILogger<BookingCreatedDomainEventHandler> _logger;
    private readonly IIntegrationEventBus _integrationEventBus;

    public BookingCreatedDomainEventHandler(
        ILogger<BookingCreatedDomainEventHandler> logger,
        IIntegrationEventBus integrationEventBus)
    {
        _logger = logger;
        _integrationEventBus = integrationEventBus;
    }

    public async Task HandleAsync(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "📅 Processing BookingCreatedDomainEvent for BookingId: {BookingId}",
            domainEvent.BookingId);

        try
        {
            // 1. Publicar integration event para actualizar inventario en CatalogService
            await PublishUpdateEventInventoryEvent(domainEvent, cancellationToken);

            // 2. Publicar integration event para notificaciones
            await PublishBookingNotificationEvent(domainEvent, cancellationToken);

            // 3. Publicar integration event para preparar datos de pago
            await PublishPreparePaymentEvent(domainEvent, cancellationToken);

            _logger.LogInformation(
                "✅ Successfully processed BookingCreatedDomainEvent for BookingId: {BookingId}",
                domainEvent.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "💥 Failed to process BookingCreatedDomainEvent for BookingId: {BookingId}",
                domainEvent.BookingId);
            throw;
        }
    }

    private async Task PublishUpdateEventInventoryEvent(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new UpdateEventInventoryIntegrationEvent(
            EventId: domainEvent.EventId,
            BookingId: domainEvent.BookingId,
            QuantityReduced: domainEvent.Quantity,
            RequestedAt: DateTime.UtcNow
        );

        _logger.LogInformation(
            "🎫 Publishing UpdateEventInventoryIntegrationEvent - EventId: {EventId}, Quantity: {Quantity}",
            domainEvent.EventId, domainEvent.Quantity);

        await _integrationEventBus.PublishAsync(integrationEvent);
    }

    private async Task PublishBookingNotificationEvent(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var integrationEvent = new SendBookingNotificationIntegrationEvent(
            BookingId: domainEvent.BookingId,
            UserId: domainEvent.UserId,
            EventId: domainEvent.EventId,
            Quantity: domainEvent.Quantity,
            TotalAmount: domainEvent.TotalAmount,
            NotificationType: "BookingCreated",
            RequestedAt: DateTime.UtcNow
        );

        _logger.LogInformation(
            "📧 Publishing SendBookingNotificationIntegrationEvent - BookingId: {BookingId}, UserId: {UserId}",
            domainEvent.BookingId, domainEvent.UserId);

        await _integrationEventBus.PublishAsync(integrationEvent);
    }

    private async Task PublishPreparePaymentEvent(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Solo preparar pago si el monto es mayor a 0
        if (domainEvent.TotalAmount > 0)
        {
            var integrationEvent = new PreparePaymentDataIntegrationEvent(
                BookingId: domainEvent.BookingId,
                Amount: domainEvent.TotalAmount,
                Currency: "EUR", // Se podría obtener de configuración
                UserId: domainEvent.UserId,
                ExpiresAt: DateTime.UtcNow.AddMinutes(15), // 15 minutos para completar el pago
                RequestedAt: DateTime.UtcNow
            );

            _logger.LogInformation(
                "💰 Publishing PreparePaymentDataIntegrationEvent - BookingId: {BookingId}, Amount: {Amount}",
                domainEvent.BookingId, domainEvent.TotalAmount);

            await _integrationEventBus.PublishAsync(integrationEvent);
        }
        else
        {
            _logger.LogInformation(
                "💰 Skipping payment preparation for BookingId: {BookingId} - Amount is {Amount}",
                domainEvent.BookingId, domainEvent.TotalAmount);
        }
    }
}