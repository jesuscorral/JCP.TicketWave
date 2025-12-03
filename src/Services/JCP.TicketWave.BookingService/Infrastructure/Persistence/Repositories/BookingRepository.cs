using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.BookingService.Domain.Models;
using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Infrastructure.Persistence;

namespace JCP.TicketWave.BookingService.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.EventId == eventId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Include(b => b.Tickets)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetExpiredBookingsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Bookings
            .Where(b => b.ExpiresAt.HasValue && b.ExpiresAt.Value < now && b.Status == BookingStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync(cancellationToken);
        return booking;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .CountAsync(b => b.UserId == userId, cancellationToken);
    }

    public async Task<int> CountByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .CountAsync(b => b.EventId == eventId, cancellationToken);
    }

    public async Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? userId = null,
        Guid? eventId = null,
        BookingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings.Include(b => b.Tickets).AsQueryable();

        // Apply filters
        if (userId.HasValue)
            query = query.Where(b => b.UserId == userId.Value);

        if (eventId.HasValue)
            query = query.Where(b => b.EventId == eventId.Value);

        if (status.HasValue)
            query = query.Where(b => b.Status == status.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}