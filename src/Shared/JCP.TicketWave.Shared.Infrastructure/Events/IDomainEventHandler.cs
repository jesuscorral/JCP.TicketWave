using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.Shared.Infrastructure.Events;

/// <summary>
/// Interfaz para handlers de eventos de dominio
/// </summary>
/// <typeparam name="TDomainEvent">Tipo de evento de dominio</typeparam>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Maneja el evento de dominio
    /// </summary>
    /// <param name="domainEvent">Evento a manejar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}