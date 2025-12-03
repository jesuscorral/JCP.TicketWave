using JCP.TicketWave.BookingService.Domain.Events;
using JCP.TicketWave.Shared.Infrastructure.Events;

namespace JCP.TicketWave.BookingService.Application.EventHandlers;

/// <summary>
/// Handler de integración que responde a eventos de booking creado
/// Envía comandos a otros servicios para procesar el booking
/// </summary>
public class BookingCreatedIntegrationEventHandler : IDomainEventHandler<BookingCreatedIntegrationEvent>
{
    private readonly ILogger<BookingCreatedIntegrationEventHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public BookingCreatedIntegrationEventHandler(
        ILogger<BookingCreatedIntegrationEventHandler> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task HandleAsync(BookingCreatedIntegrationEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing BookingCreatedIntegrationEvent for BookingId: {BookingId}",
            domainEvent.BookingId);

        try
        {
            // 1. Reducir inventario disponible en CatalogService
            await UpdateEventInventory(domainEvent.EventId, domainEvent.Quantity, cancellationToken);

            // 2. Notificar a NotificationService para envío de confirmación
            await SendBookingNotification(domainEvent, cancellationToken);

            // 3. Preparar datos para PaymentService si aplica
            await PreparePaymentData(domainEvent, cancellationToken);

            _logger.LogInformation(
                "Successfully processed BookingCreatedIntegrationEvent for BookingId: {BookingId}",
                domainEvent.BookingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to process BookingCreatedIntegrationEvent for BookingId: {BookingId}",
                domainEvent.BookingId);
            
            // Aquí podrías implementar retry logic o compensación
            throw;
        }
    }

    private async Task UpdateEventInventory(Guid eventId, int quantity, CancellationToken cancellationToken)
    {
        var catalogClient = _httpClientFactory.CreateClient("CatalogService");
        
        // Simular llamada HTTP para actualizar inventario
        var request = new
        {
            EventId = eventId,
            QuantityReduced = quantity
        };

        _logger.LogInformation(
            "Updating inventory for EventId: {EventId}, Quantity: {Quantity}",
            eventId, quantity);

        // await catalogClient.PutAsJsonAsync($"/api/events/{eventId}/reduce-inventory", request, cancellationToken);
        await Task.CompletedTask; // Placeholder
    }

    private async Task SendBookingNotification(BookingCreatedIntegrationEvent domainEvent, CancellationToken cancellationToken)
    {
        var notificationClient = _httpClientFactory.CreateClient("NotificationService");
        
        var notificationData = new
        {
            BookingId = domainEvent.BookingId,
            CustomerEmail = domainEvent.CustomerEmail,
            EventId = domainEvent.EventId,
            Quantity = domainEvent.Quantity,
            TotalAmount = domainEvent.TotalAmount,
            Type = "BookingCreated"
        };

        _logger.LogInformation(
            "Sending booking notification for BookingId: {BookingId}, Email: {Email}",
            domainEvent.BookingId, domainEvent.CustomerEmail);

        // await notificationClient.PostAsJsonAsync("/api/notifications/booking-created", notificationData, cancellationToken);
        await Task.CompletedTask; // Placeholder
    }

    private async Task PreparePaymentData(BookingCreatedIntegrationEvent domainEvent, CancellationToken cancellationToken)
    {
        // Si el booking requiere pago, preparar datos en PaymentService
        if (domainEvent.TotalAmount > 0)
        {
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");
            
            var paymentData = new
            {
                BookingId = domainEvent.BookingId,
                Amount = domainEvent.TotalAmount,
                Currency = "EUR", // Obtener de configuración o del evento
                ExpiresAt = domainEvent.ExpiresAt
            };

            _logger.LogInformation(
                "Preparing payment for BookingId: {BookingId}, Amount: {Amount}",
                domainEvent.BookingId, domainEvent.TotalAmount);

            // await paymentClient.PostAsJsonAsync("/api/payments/prepare", paymentData, cancellationToken);
            await Task.CompletedTask; // Placeholder
        }
    }
}