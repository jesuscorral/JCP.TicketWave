namespace JCP.TicketWave.Shared.Infrastructure.Domain;

/// <summary>
/// Interfaz para eventos de dominio
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Timestamp cuando ocurrió el evento
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Record base para eventos de dominio
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; private init; } = DateTime.UtcNow;
}

/// <summary>
/// Interfaz para eventos de integración entre servicios
/// </summary>
public interface IIntegrationEvent : IDomainEvent
{
    string EventType { get; }
    string Source { get; }
}

/// <summary>
/// Record base para eventos de integración
/// </summary>
public abstract record IntegrationEvent : DomainEvent, IIntegrationEvent
{
    public abstract string EventType { get; }
    public abstract string Source { get; }
}