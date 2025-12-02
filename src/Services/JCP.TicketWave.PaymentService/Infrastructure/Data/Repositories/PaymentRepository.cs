using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Data;
using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Infrastructure.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(PaymentDbContext context, ILogger<PaymentRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.Events.OrderBy(e => e.OccurredAt))
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment {PaymentId}", id);
            throw;
        }
    }

    public async Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.Events.OrderBy(e => e.OccurredAt))
                .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment for booking {BookingId}", bookingId);
            throw;
        }
    }

    public async Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.Events.OrderBy(e => e.OccurredAt))
                .FirstOrDefaultAsync(p => p.ExternalPaymentId == externalPaymentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment with external ID {ExternalPaymentId}", externalPaymentId);
            throw;
        }
    }

    public async Task<IEnumerable<Payment>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments with status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.PaymentMethod)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for date range {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created payment {PaymentId} for booking {BookingId}", payment.Id, payment.BookingId);
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment {PaymentId}", payment.Id);
            throw;
        }
    }

    public async Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated payment {PaymentId} with status {Status}", payment.Id, payment.Status);
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update payment {PaymentId}", payment.Id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments.AnyAsync(p => p.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if payment {PaymentId} exists", id);
            throw;
        }
    }

    public async Task<bool> BookingHasPaymentAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Payments.AnyAsync(p => p.BookingId == bookingId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if booking {BookingId} has payment", bookingId);
            throw;
        }
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
        try
        {
            var query = _context.Payments
                .Include(p => p.PaymentMethod)
                .AsQueryable();

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged payments");
            throw;
        }
    }

    public async Task<decimal> GetTotalAmountByTenantAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Payments
                .Where(p => p.TenantId == tenantId && p.Status == PaymentStatus.Succeeded);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            return await query.SumAsync(p => p.Amount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total amount for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<Dictionary<PaymentStatus, int>> GetPaymentStatisticsAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Payments.AsQueryable();

            if (!string.IsNullOrEmpty(tenantId))
                query = query.Where(p => p.TenantId == tenantId);

            return await query
                .GroupBy(p => p.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment statistics");
            throw;
        }
    }
}