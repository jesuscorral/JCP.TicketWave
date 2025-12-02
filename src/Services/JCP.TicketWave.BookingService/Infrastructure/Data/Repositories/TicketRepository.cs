using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.BookingService.Domain.Entities;
using JCP.TicketWave.BookingService.Domain.Enums;
using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Infrastructure.Data;

namespace JCP.TicketWave.BookingService.Infrastructure.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly BookingDbContext _context;

    public TicketRepository(BookingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Booking)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.EventId == eventId)
            .OrderBy(t => t.TicketType)
            .ThenBy(t => t.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.BookingId == bookingId)
            .OrderBy(t => t.TicketType)
            .ThenBy(t => t.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.Status == status)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Reserved && 
                       t.ReservedUntil.HasValue && 
                       t.ReservedUntil.Value < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetAvailableByEventIdAsync(
        Guid eventId, 
        string? ticketType = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tickets
            .Where(t => t.EventId == eventId && t.Status == TicketStatus.Available);

        if (!string.IsNullOrWhiteSpace(ticketType))
        {
            query = query.Where(t => t.TicketType == ticketType);
        }

        return await query
            .OrderBy(t => t.TicketType)
            .ThenBy(t => t.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket> CreateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task<Ticket> UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        return ticket;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets.FindAsync(new object[] { id }, cancellationToken);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<int> CountAvailableByEventIdAsync(
        Guid eventId, 
        string? ticketType = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Where(t => t.EventId == eventId && t.Status == TicketStatus.Available);

        if (!string.IsNullOrWhiteSpace(ticketType))
        {
            query = query.Where(t => t.TicketType == ticketType);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountByStatusAsync(Guid eventId, TicketStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AsNoTracking()
            .CountAsync(t => t.EventId == eventId && t.Status == status, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> CreateManyAsync(
        IEnumerable<Ticket> tickets, 
        CancellationToken cancellationToken = default)
    {
        var ticketList = tickets.ToList();
        _context.Tickets.AddRange(ticketList);
        await _context.SaveChangesAsync(cancellationToken);
        return ticketList;
    }

    public async Task<IEnumerable<Ticket>> UpdateManyAsync(
        IEnumerable<Ticket> tickets, 
        CancellationToken cancellationToken = default)
    {
        var ticketList = tickets.ToList();
        _context.Tickets.UpdateRange(ticketList);
        await _context.SaveChangesAsync(cancellationToken);
        return ticketList;
    }

    public async Task<int> ReleaseExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        var expiredTickets = await _context.Tickets
            .Where(t => t.Status == TicketStatus.Reserved && 
                       t.ReservedUntil.HasValue && 
                       t.ReservedUntil.Value < now)
            .ToListAsync(cancellationToken);

        foreach (var ticket in expiredTickets)
        {
            ticket.Release();
        }

        if (expiredTickets.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return expiredTickets.Count;
    }

    public async Task<IEnumerable<Ticket>> ReserveTicketsAsync(
        Guid eventId,
        Guid bookingId,
        int quantity,
        string? ticketType = null,
        TimeSpan? reservationDuration = null,
        CancellationToken cancellationToken = default)
    {
        // Start a transaction for atomic operation
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Get available tickets with row-level locking
            var query = _context.Tickets
                .Where(t => t.EventId == eventId && t.Status == TicketStatus.Available);

            if (!string.IsNullOrWhiteSpace(ticketType))
            {
                query = query.Where(t => t.TicketType == ticketType);
            }

            var availableTickets = await query
                .OrderBy(t => t.TicketType)
                .ThenBy(t => t.SeatNumber)
                .Take(quantity)
                .ToListAsync(cancellationToken);

            if (availableTickets.Count < quantity)
            {
                throw new InvalidOperationException($"Only {availableTickets.Count} tickets available, but {quantity} requested");
            }

            // Reserve the tickets
            var duration = reservationDuration ?? TimeSpan.FromMinutes(15);
            foreach (var ticket in availableTickets)
            {
                ticket.Reserve(bookingId, duration);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return availableTickets;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}