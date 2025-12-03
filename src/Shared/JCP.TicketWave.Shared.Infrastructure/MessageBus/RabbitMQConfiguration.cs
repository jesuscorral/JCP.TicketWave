namespace JCP.TicketWave.Shared.Infrastructure.MessageBus;

/// <summary>
/// Configuración para RabbitMQ
/// </summary>
public class RabbitMQConfiguration
{
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// Cadena de conexión de RabbitMQ
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Host de RabbitMQ
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Puerto de RabbitMQ
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Virtual Host
    /// </summary>
    public string VirtualHost { get; set; } = "/ticketwave";

    /// <summary>
    /// Usuario
    /// </summary>
    public string Username { get; set; } = "admin";

    /// <summary>
    /// Contraseña
    /// </summary>
    public string Password { get; set; } = "admin123";

    /// <summary>
    /// Exchange principal para eventos
    /// </summary>
    public string Exchange { get; set; } = "events.topic";

    /// <summary>
    /// Exchange para dead letter
    /// </summary>
    public string DeadLetterExchange { get; set; } = "events.dlx";

    /// <summary>
    /// Prefijo para las colas
    /// </summary>
    public string QueuePrefix { get; set; } = "events.";

    /// <summary>
    /// Número máximo de reintentos
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Timeout para las conexiones en segundos
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// TTL de mensajes en milisegundos (24 horas por defecto)
    /// </summary>
    public int MessageTtl { get; set; } = 86400000;

    /// <summary>
    /// Habilitar SSL
    /// </summary>
    public bool UseSsl { get; set; } = false;
}