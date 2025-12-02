using JCP.TicketWave.BookingService.Domain.Entities;
using JCP.TicketWave.BookingService.Domain.Enums;

namespace JCP.TicketWave.BookingService.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetAvailableByEventIdAsync(Guid eventId, string? ticketType = null, CancellationToken cancellationToken = default);
    Task<Ticket> CreateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAvailableByEventIdAsync(Guid eventId, string? ticketType = null, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(Guid eventId, TicketStatus status, CancellationToken cancellationToken = default);
    
    // Bulk operations for performance
    Task<IEnumerable<Ticket>> CreateManyAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> UpdateManyAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    Task<int> ReleaseExpiredReservationsAsync(CancellationToken cancellationToken = default);
    
    // Reservation operations
    Task<IEnumerable<Ticket>> ReserveTicketsAsync(
        Guid eventId,
        Guid bookingId,
        int quantity,
        string? ticketType = null,
        TimeSpan? reservationDuration = null,
        CancellationToken cancellationToken = default);
}