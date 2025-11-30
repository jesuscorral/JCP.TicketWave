namespace JCP.TicketWave.Shared.Contracts.Events;

public record EventCreated(
    Guid EventId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Venue,
    string Category,
    decimal Price,
    int TotalTickets,
    DateTime CreatedAt);

public record EventUpdated(
    Guid EventId,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Venue,
    string Category,
    decimal Price,
    DateTime UpdatedAt);

public record EventCancelled(
    Guid EventId,
    string Reason,
    DateTime CancelledAt);