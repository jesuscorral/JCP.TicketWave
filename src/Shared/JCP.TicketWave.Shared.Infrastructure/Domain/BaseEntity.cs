namespace JCP.TicketWave.Shared.Infrastructure.Domain;

/// <summary>
/// Clase base para entidades con identificador único
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Fecha de creación de la entidad
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Fecha de última modificación de la entidad
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza la fecha de modificación
    /// </summary>
    protected void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Compara entidades por su Id
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (BaseEntity)obj;
        return Id == other.Id;
    }

    /// <summary>
    /// Genera hash code basado en el Id
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Implementa el operador de igualdad
    /// </summary>
    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Implementa el operador de desigualdad
    /// </summary>
    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}