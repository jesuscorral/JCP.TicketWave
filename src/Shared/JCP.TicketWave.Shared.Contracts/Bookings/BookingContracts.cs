namespace JCP.TicketWave.Shared.Contracts.Bookings;

public record BookingCreated(
    Guid BookingId,
    string BookingReference,
    Guid EventId,
    string UserId,
    int TicketCount,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime ExpiresAt);

public record BookingConfirmed(
    Guid BookingId,
    string BookingReference,
    Guid PaymentId,
    DateTime ConfirmedAt);

public record BookingCancelled(
    Guid BookingId,
    string BookingReference,
    string Reason,
    DateTime CancelledAt);

public record BookingExpired(
    Guid BookingId,
    string BookingReference,
    DateTime ExpiredAt);