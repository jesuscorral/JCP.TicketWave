namespace JCP.TicketWave.Shared.Infrastructure.Domain;

/// <summary>
/// Excepción de dominio para errores de lógica de negocio
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}