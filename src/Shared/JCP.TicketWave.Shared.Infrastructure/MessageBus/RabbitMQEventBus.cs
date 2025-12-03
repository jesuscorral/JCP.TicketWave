using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.Shared.Infrastructure.MessageBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace JCP.TicketWave.Shared.Infrastructure.MessageBus;

/// <summary>
/// Implementación de bus de eventos de integración usando RabbitMQ
/// </summary>
public class RabbitMQEventBus : IIntegrationEventBus, IDisposable
{
    private readonly RabbitMQConfiguration _configuration;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly ConcurrentDictionary<string, Func<string, Task>> _handlers;
    
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public RabbitMQEventBus(
        IOptions<RabbitMQConfiguration> configuration,
        ILogger<RabbitMQEventBus> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _handlers = new ConcurrentDictionary<string, Func<string, Task>>();
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        if (integrationEvent == null) 
            throw new ArgumentNullException(nameof(integrationEvent));

        await EnsureConnectedAsync();

        var routingKey = integrationEvent.EventType;
        var message = JsonSerializer.Serialize(integrationEvent, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = integrationEvent.Id.ToString();
        properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)integrationEvent.OccurredOn).ToUnixTimeSeconds());
        properties.Type = typeof(T).Name;
        properties.Headers = new Dictionary<string, object>
        {
            ["source"] = integrationEvent.Source,
            ["eventType"] = integrationEvent.EventType
        };

        try
        {
            _channel.BasicPublish(
                exchange: _configuration.Exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published integration event {EventId} of type {EventType} with routing key {RoutingKey}",
                integrationEvent.Id,
                integrationEvent.EventType,
                routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish integration event {EventId} of type {EventType}",
                integrationEvent.Id,
                integrationEvent.EventType);
            throw;
        }
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent
    {
        await EnsureConnectedAsync();

        var eventType = GetEventType<T>();
        var queueName = $"{_configuration.QueuePrefix}{eventType}";

        // Declarar la cola si no existe
        _channel!.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-message-ttl"] = _configuration.MessageTtl,
                ["x-dead-letter-exchange"] = _configuration.DeadLetterExchange
            });

        // Vincular la cola al exchange
        _channel.QueueBind(
            queue: queueName,
            exchange: _configuration.Exchange,
            routingKey: eventType);

        // Wrapper para el handler
        async Task HandleMessage(string message)
        {
            try
            {
                var integrationEvent = JsonSerializer.Deserialize<T>(message, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                if (integrationEvent != null)
                {
                    await handler(integrationEvent);
                    _logger.LogInformation("Successfully handled event of type {EventType}", typeof(T).Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event of type {EventType}: {Message}", typeof(T).Name, message);
                throw;
            }
        }

        _handlers.TryAdd(eventType, HandleMessage);

        // Configurar el consumer
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var routingKey = ea.RoutingKey;

            try
            {
                if (_handlers.TryGetValue(routingKey, out var messageHandler))
                {
                    await messageHandler(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {Queue}", queueName);
                
                // Rechazar el mensaje y enviarlo a dead letter si aplica
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Subscribed to events of type {EventType} on queue {Queue}", typeof(T).Name, queueName);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync();
        _logger.LogInformation("RabbitMQ Event Bus started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Dispose();
        _logger.LogInformation("RabbitMQ Event Bus stopped");
        await Task.CompletedTask;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            return;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host,
                Port = _configuration.Port,
                VirtualHost = _configuration.VirtualHost,
                UserName = _configuration.Username,
                Password = _configuration.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(_configuration.ConnectionTimeout),
                SocketReadTimeout = TimeSpan.FromSeconds(30),
                SocketWriteTimeout = TimeSpan.FromSeconds(30),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            if (_configuration.UseSsl)
            {
                factory.Ssl.Enabled = true;
                factory.Ssl.ServerName = _configuration.Host;
            }

            _connection = factory.CreateConnection("TicketWave-EventBus");
            _channel = _connection.CreateModel();

            // Declarar exchanges principales
            _channel.ExchangeDeclare(
                exchange: _configuration.Exchange,
                type: ExchangeType.Topic,
                durable: true);

            _channel.ExchangeDeclare(
                exchange: _configuration.DeadLetterExchange,
                type: ExchangeType.Direct,
                durable: true);

            _logger.LogInformation("Successfully connected to RabbitMQ at {Host}:{Port}", 
                _configuration.Host, _configuration.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}", 
                _configuration.Host, _configuration.Port);
            throw;
        }

        await Task.CompletedTask;
    }

    private static string GetEventType<T>() where T : IIntegrationEvent
    {
        return typeof(T).Name.Replace("IntegrationEvent", "").ToLowerInvariant();
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _channel?.Close();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ channel");
        }

        try
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connection");
        }

        _disposed = true;
    }
}