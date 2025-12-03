using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.NotificationService.Domain.Events;

/// <summary>
/// Evento disparado cuando se envía un email de confirmación
/// </summary>
public sealed record BookingConfirmationEmailSentDomainEvent(
    Guid BookingId,
    string CustomerEmail,
    string EventName,
    DateTime SentAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando falla el envío de un email
/// </summary>
public sealed record EmailSendFailedDomainEvent(
    Guid BookingId,
    string CustomerEmail,
    string EmailType,
    string FailureReason,
    DateTime FailedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se genera un PDF de tickets
/// </summary>
public sealed record TicketPdfGeneratedDomainEvent(
    Guid BookingId,
    string CustomerEmail,
    string PdfPath,
    int TicketCount,
    DateTime GeneratedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando falla la generación de PDF
/// </summary>
public sealed record PdfGenerationFailedDomainEvent(
    Guid BookingId,
    string CustomerEmail,
    string FailureReason,
    DateTime FailedAt
) : DomainEvent;

/// <summary>
/// Evento disparado cuando se envía un recordatorio de evento
/// </summary>
public sealed record EventReminderSentDomainEvent(
    Guid EventId,
    string EventName,
    List<string> RecipientEmails,
    DateTime EventDate,
    DateTime SentAt
) : DomainEvent;