using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Entities;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Data;

namespace JCP.TicketWave.PaymentService.Infrastructure.Data.Repositories;

public class RefundRepository : IRefundRepository
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<RefundRepository> _logger;

    public RefundRepository(PaymentDbContext context, ILogger<RefundRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Refund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Include(r => r.Payment)
                .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refund {RefundId}", id);
            throw;
        }
    }

    public async Task<Refund?> GetByExternalIdAsync(string externalRefundId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Include(r => r.Payment)
                .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(r => r.ExternalRefundId == externalRefundId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refund with external ID {ExternalRefundId}", externalRefundId);
            throw;
        }
    }

    public async Task<IEnumerable<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Where(r => r.PaymentId == paymentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refunds for payment {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<IEnumerable<Refund>> GetByStatusAsync(RefundStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Include(r => r.Payment)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refunds with status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Refund>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Include(r => r.Payment)
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refunds for date range {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<Refund> CreateAsync(Refund refund, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created refund {RefundId} for payment {PaymentId}", refund.Id, refund.PaymentId);
            return refund;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create refund {RefundId}", refund.Id);
            throw;
        }
    }

    public async Task<Refund> UpdateAsync(Refund refund, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Refunds.Update(refund);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated refund {RefundId} with status {Status}", refund.Id, refund.Status);
            return refund;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update refund {RefundId}", refund.Id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds.AnyAsync(r => r.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if refund {RefundId} exists", id);
            throw;
        }
    }

    public async Task<decimal> GetTotalRefundAmountForPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Refunds
                .Where(r => r.PaymentId == paymentId && r.Status == RefundStatus.Succeeded)
                .SumAsync(r => r.Amount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total refund amount for payment {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<(IEnumerable<Refund> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        RefundStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Refunds
                .Include(r => r.Payment)
                .ThenInclude(p => p.PaymentMethod)
                .AsQueryable();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged refunds");
            throw;
        }
    }
}