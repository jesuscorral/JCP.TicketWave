using JCP.TicketWave.Shared.Infrastructure.Events;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using JCP.TicketWave.CatalogService.Domain.Interfaces;

namespace JCP.TicketWave.CatalogService.Application.EventHandlers;

/// <summary>
/// Handler para eventos de booking creado desde BookingService
/// Reduce el inventario disponible del evento
/// </summary>
public class BookingCreatedIntegrationEventHandler
{
    private readonly ILogger<BookingCreatedIntegrationEventHandler> _logger;
    private readonly IEventRepository _eventRepository;

    public BookingCreatedIntegrationEventHandler(
        ILogger<BookingCreatedIntegrationEventHandler> logger,
        IEventRepository eventRepository)
    {
        _logger = logger;
        _eventRepository = eventRepository;
    }

    public async Task HandleAsync(BookingCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling BookingCreatedIntegrationEvent for EventId: {EventId}, Quantity: {Quantity}",
            integrationEvent.EventId,
            integrationEvent.Quantity);

        try
        {
            // Obtener el evento
            var eventEntity = await _eventRepository.GetByIdAsync(integrationEvent.EventId);
            if (eventEntity == null)
            {
                _logger.LogWarning("Event {EventId} not found", integrationEvent.EventId);
                return;
            }

            // Reducir inventario disponible
            var newAvailableTickets = eventEntity.AvailableTickets - integrationEvent.Quantity;
            eventEntity.UpdateAvailableTickets(Math.Max(0, newAvailableTickets));

            // Guardar cambios
            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation(
                "Successfully reduced inventory for EventId: {EventId}, New available tickets: {AvailableTickets}",
                integrationEvent.EventId,
                eventEntity.AvailableTickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to handle BookingCreatedIntegrationEvent for EventId: {EventId}",
                integrationEvent.EventId);
            throw;
        }
    }
}

/// <summary>
/// Handler para eventos de booking cancelado desde BookingService
/// Restaura el inventario disponible del evento
/// </summary>
public class BookingCancelledIntegrationEventHandler
{
    private readonly ILogger<BookingCancelledIntegrationEventHandler> _logger;
    private readonly IEventRepository _eventRepository;

    public BookingCancelledIntegrationEventHandler(
        ILogger<BookingCancelledIntegrationEventHandler> logger,
        IEventRepository eventRepository)
    {
        _logger = logger;
        _eventRepository = eventRepository;
    }

    public async Task HandleAsync(BookingCancelledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling BookingCancelledIntegrationEvent for EventId: {EventId}, BookingId: {BookingId}",
            integrationEvent.EventId,
            integrationEvent.BookingId);

        try
        {
            // Obtener el evento
            var eventEntity = await _eventRepository.GetByIdAsync(integrationEvent.EventId);
            if (eventEntity == null)
            {
                _logger.LogWarning("Event {EventId} not found", integrationEvent.EventId);
                return;
            }

            // TODO: Obtener la cantidad de tickets del booking cancelado
            // Por simplicidad, asumimos que necesitamos obtener esta información de alguna manera
            // En un escenario real, podrías incluir la cantidad en el evento de integración
            var ticketsToRestore = 1; // Placeholder

            // Restaurar inventario disponible
            eventEntity.UpdateAvailableTickets(eventEntity.AvailableTickets + ticketsToRestore);

            // Guardar cambios
            await _eventRepository.UpdateAsync(eventEntity);

            _logger.LogInformation(
                "Successfully restored inventory for EventId: {EventId}, Restored tickets: {RestoredTickets}",
                integrationEvent.EventId,
                ticketsToRestore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to handle BookingCancelledIntegrationEvent for EventId: {EventId}",
                integrationEvent.EventId);
            throw;
        }
    }
}