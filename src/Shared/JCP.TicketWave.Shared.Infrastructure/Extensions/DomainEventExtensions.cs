using JCP.TicketWave.Shared.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;

namespace JCP.TicketWave.Shared.Infrastructure.Extensions;

/// <summary>
/// Extensiones para configurar eventos de dominio
/// </summary>
public static class DomainEventExtensions
{
    /// <summary>
    /// Agrega soporte para eventos de dominio
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services)
    {
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// Agrega handlers de eventos de dominio automáticamente
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="assemblies">Assemblies donde buscar handlers</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && 
                           t.GetInterfaces().Any(i => 
                               i.IsGenericType && 
                               i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaceTypes = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                               i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

                foreach (var interfaceType in interfaceTypes)
                {
                    services.AddScoped(interfaceType, handlerType);
                }
            }
        }

        return services;
    }
}