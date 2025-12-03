using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.Shared.Infrastructure.MessageBus;

/// <summary>
/// Interfaz para el bus de eventos de integración
/// </summary>
public interface IIntegrationEventBus
{
    /// <summary>
    /// Publica un evento de integración
    /// </summary>
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent;

    /// <summary>
    /// Suscribe un manejador a un tipo específico de evento
    /// </summary>
    Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent;

    /// <summary>
    /// Inicia el bus de mensajes y sus suscripciones
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Detiene el bus de mensajes
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}