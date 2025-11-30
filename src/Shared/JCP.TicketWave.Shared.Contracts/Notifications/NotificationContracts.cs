namespace JCP.TicketWave.Shared.Contracts.Notifications;

public record SendEmailNotification(
    string ToEmail,
    string Subject,
    string Body,
    NotificationPriority Priority = NotificationPriority.Normal,
    Dictionary<string, object>? Metadata = null);

public record GeneratePdfTicket(
    Guid BookingId,
    string BookingReference,
    string CustomerName,
    string CustomerEmail,
    string EventTitle,
    DateTime EventDate,
    string Venue,
    IEnumerable<TicketInfo> Tickets);

public record TicketInfo(
    string TicketNumber,
    string SeatNumber,
    string Category);

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}