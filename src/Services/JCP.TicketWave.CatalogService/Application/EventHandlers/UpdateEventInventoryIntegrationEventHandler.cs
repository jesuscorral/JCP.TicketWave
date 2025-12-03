using JCP.TicketWave.Shared.Infrastructure.Events;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.Shared.Infrastructure.Messaging;

namespace JCP.TicketWave.CatalogService.Application.EventHandlers;

/// <summary>
/// Handler que procesa solicitudes de actualización de inventario desde BookingService
/// </summary>
public class UpdateEventInventoryIntegrationEventHandler : IMessageHandler<UpdateEventInventoryIntegrationEvent>
{
    private readonly IEventRepository _eventRepository;
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly ILogger<UpdateEventInventoryIntegrationEventHandler> _logger;

    public UpdateEventInventoryIntegrationEventHandler(
        IEventRepository eventRepository,
        IIntegrationEventBus integrationEventBus,
        ILogger<UpdateEventInventoryIntegrationEventHandler> logger)
    {
        _eventRepository = eventRepository;
        _integrationEventBus = integrationEventBus;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateEventInventoryIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "🎫 Processing UpdateEventInventoryIntegrationEvent - EventId: {EventId}, BookingId: {BookingId}, Quantity: {Quantity}",
                integrationEvent.EventId, integrationEvent.BookingId, integrationEvent.QuantityReduced);

            // Obtener el evento
            var eventEntity = await _eventRepository.GetByIdAsync(integrationEvent.EventId);
            if (eventEntity == null)
            {
                _logger.LogWarning("Event {EventId} not found", integrationEvent.EventId);
                return;
            }

            // Guardar el inventario anterior
            var previousAvailableTickets = eventEntity.AvailableTickets;

            // Reducir inventario
            var newAvailableTickets = Math.Max(0, previousAvailableTickets - integrationEvent.QuantityReduced);
            eventEntity.UpdateAvailableTickets(newAvailableTickets);

            // Actualizar en el repositorio
            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation(
                "✅ Inventory updated successfully - EventId: {EventId}, Previous: {Previous}, New: {New}",
                integrationEvent.EventId, previousAvailableTickets, newAvailableTickets);

            // Publicar evento de inventario actualizado
            var inventoryUpdatedEvent = new InventoryUpdatedIntegrationEvent(
                EventId: integrationEvent.EventId,
                BookingId: integrationEvent.BookingId,
                PreviousAvailableTickets: previousAvailableTickets,
                CurrentAvailableTickets: newAvailableTickets,
                UpdateType: "BOOKING_RESERVATION",
                UpdatedAt: DateTime.UtcNow
            );

            await _integrationEventBus.PublishAsync(inventoryUpdatedEvent);
            
            _logger.LogInformation(
                "📊 Published InventoryUpdatedIntegrationEvent - EventId: {EventId}",
                integrationEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "💥 Error updating inventory for EventId: {EventId}, BookingId: {BookingId}",
                integrationEvent.EventId, integrationEvent.BookingId);
            throw;
        }
    }
}