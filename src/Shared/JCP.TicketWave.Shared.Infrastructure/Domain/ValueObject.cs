namespace JCP.TicketWave.Shared.Infrastructure.Domain;

/// <summary>
/// Clase base para objetos de valor en DDD
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Obtiene los componentes atómicos del objeto de valor
    /// </summary>
    /// <returns>Colección de componentes que definen la igualdad</returns>
    protected abstract IEnumerable<object> GetAtomicValues();

    /// <summary>
    /// Compara objetos de valor por sus componentes atómicos
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    /// <summary>
    /// Genera hash code basado en los componentes atómicos
    /// </summary>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Implementa el operador de igualdad
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
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
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}