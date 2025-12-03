namespace JCP.TicketWave.Shared.Infrastructure.Domain;

/// <summary>
/// Clase base para agregados de dominio que pueden publicar eventos
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Eventos de dominio pendientes de publicar
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Agrega un evento de dominio al agregado
    /// </summary>
    /// <param name="domainEvent">Evento a agregar</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
        UpdateTimestamp();
    }

    /// <summary>
    /// Remueve un evento de dominio específico
    /// </summary>
    /// <param name="domainEvent">Evento a remover</param>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Limpia todos los eventos de dominio
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marca el agregado como modificado
    /// </summary>
    protected void MarkAsModified()
    {
        UpdateTimestamp();
    }
}