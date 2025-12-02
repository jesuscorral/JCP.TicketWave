using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Models;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Persistence;

namespace JCP.TicketWave.PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
        return payment;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> BookingHasPaymentAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments.AnyAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<(IEnumerable<Payment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? tenantId = null,
        PaymentStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Include(p => p.PaymentMethod).AsQueryable();

        if (!string.IsNullOrEmpty(tenantId))
            query = query.Where(p => p.TenantId == tenantId);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<decimal> GetTotalAmountByTenantAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.Where(p => p.TenantId == tenantId && p.Status == PaymentStatus.Succeeded);

        if (startDate.HasValue)
            query = query.Where(p => p.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.CreatedAt <= endDate.Value);

        return await query.SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<Dictionary<PaymentStatus, int>> GetPaymentStatisticsAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Payments.AsQueryable();

        if (!string.IsNullOrEmpty(tenantId))
            query = query.Where(p => p.TenantId == tenantId);

        return await query
            .GroupBy(p => p.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }
}