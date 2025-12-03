using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JCP.TicketWave.Shared.Infrastructure.Extensions;

/// <summary>
/// Extensiones para configurar RabbitMQ
/// </summary>
public static class RabbitMQExtensions
{
    /// <summary>
    /// Agrega RabbitMQ como bus de eventos de integración
    /// </summary>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración
        services.Configure<RabbitMQConfiguration>(
            configuration.GetSection(RabbitMQConfiguration.SectionName));

        // Servicios
        services.AddSingleton<IIntegrationEventBus, RabbitMQEventBus>();
        services.AddHostedService<RabbitMQHostedService>();

        return services;
    }

    /// <summary>
    /// Agrega RabbitMQ con configuración específica
    /// </summary>
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, Action<RabbitMQConfiguration> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IIntegrationEventBus, RabbitMQEventBus>();
        services.AddHostedService<RabbitMQHostedService>();

        return services;
    }
}

/// <summary>
/// Servicio hospedado para gestionar el ciclo de vida de RabbitMQ
/// </summary>
internal class RabbitMQHostedService : IHostedService
{
    private readonly IIntegrationEventBus _eventBus;

    public RabbitMQHostedService(IIntegrationEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _eventBus.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _eventBus.StopAsync(cancellationToken);
    }
}