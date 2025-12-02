using JCP.TicketWave.BookingService.Domain.Entities;
using JCP.TicketWave.BookingService.Domain.Enums;

namespace JCP.TicketWave.BookingService.Domain.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetExpiredBookingsAsync(CancellationToken cancellationToken = default);
    Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    
    // Pagination support
    Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? userId = null,
        Guid? eventId = null,
        BookingStatus? status = null,
        CancellationToken cancellationToken = default);
}