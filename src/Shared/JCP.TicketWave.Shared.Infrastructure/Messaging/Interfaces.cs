namespace JCP.TicketWave.Shared.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(T message, string topic, CancellationToken cancellationToken = default) where T : class;
}

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public interface IMessageHandler<in T> where T : class
{
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}

public record MessageMetadata(
    string MessageId,
    string CorrelationId,
    DateTime Timestamp,
    string Source,
    Dictionary<string, object>? Properties = null);