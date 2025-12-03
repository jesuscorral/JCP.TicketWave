using JCP.TicketWave.Shared.Infrastructure.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace JCP.TicketWave.Shared.Infrastructure.Events;

/// <summary>
/// Interfaz para el despachador de eventos de dominio
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Despacha un evento de dominio específico
    /// </summary>
    /// <param name="domainEvent">Evento a despachar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Despacha múltiples eventos de dominio
    /// </summary>
    /// <param name="domainEvents">Eventos a despachar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Task</returns>
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementación del despachador de eventos de dominio
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        
        var handlers = _serviceProvider.GetServices(handlerType);
        
        var tasks = handlers.Select(handler => 
            (Task)handlerType.GetMethod("HandleAsync")!
                .Invoke(handler, new object[] { domainEvent, cancellationToken })!);

        await Task.WhenAll(tasks);
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        var tasks = domainEvents.Select(domainEvent => DispatchAsync(domainEvent, cancellationToken));
        await Task.WhenAll(tasks);
    }
}

public static class DomainEventDispatcherExtensions
{
    /// <summary>
    /// Despacha todos los eventos de dominio de un agregado y los limpia
    /// </summary>
    public static async Task DispatchAndClearEventsAsync(
        this IDomainEventDispatcher dispatcher, 
        AggregateRoot aggregate,
        CancellationToken cancellationToken = default)
    {
        var events = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();
        await dispatcher.DispatchAsync(events, cancellationToken);
    }
}