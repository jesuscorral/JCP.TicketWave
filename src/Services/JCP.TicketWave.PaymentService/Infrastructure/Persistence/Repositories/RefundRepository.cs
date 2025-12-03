using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Models;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Persistence;

namespace JCP.TicketWave.PaymentService.Infrastructure.Persistence.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly PaymentDbContext _context;

    public RefundRepository(PaymentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Refund?> GetByExternalIdAsync(string externalRefundId, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.ExternalRefundId == externalRefundId, cancellationToken);
    }

    public async Task<IEnumerable<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Include(r => r.Payment)
            .Where(r => r.PaymentId == paymentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Refund>> GetByStatusAsync(RefundStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Include(r => r.Payment)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Refund>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Include(r => r.Payment)
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Refund> CreateAsync(Refund refund, CancellationToken cancellationToken = default)
    {
        if (refund == null)
            throw new ArgumentNullException(nameof(refund));

        _context.Refunds.Add(refund);
        await _context.SaveChangesAsync(cancellationToken);
        return refund;
    }

    public async Task<Refund> UpdateAsync(Refund refund, CancellationToken cancellationToken = default)
    {
        if (refund == null)
            throw new ArgumentNullException(nameof(refund));

        _context.Refunds.Update(refund);
        await _context.SaveChangesAsync(cancellationToken);
        return refund;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds.AnyAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<decimal> GetTotalRefundAmountForPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return await _context.Refunds
            .Where(r => r.PaymentId == paymentId && r.Status == RefundStatus.Succeeded)
            .SumAsync(r => r.Amount, cancellationToken);
    }

    public async Task<(IEnumerable<Refund> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        RefundStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Refunds.Include(r => r.Payment).AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (startDate.HasValue)
            query = query.Where(r => r.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.CreatedAt <= endDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}