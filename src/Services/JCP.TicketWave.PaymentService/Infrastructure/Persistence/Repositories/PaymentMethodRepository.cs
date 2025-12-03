using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Models;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Persistence;

namespace JCP.TicketWave.PaymentService.Infrastructure.Persistence.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly PaymentDbContext _context;

    public PaymentMethodRepository(PaymentDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task<PaymentMethod?> GetByExternalIdAsync(string externalMethodId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.ExternalMethodId == externalMethodId, cancellationToken);
    }

    public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentMethod>> GetByTenantAsync(string tenantId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.PaymentMethods.Where(pm => pm.TenantId == tenantId);
        
        if (activeOnly)
            query = query.Where(pm => pm.IsActive);

        return await query.OrderBy(pm => pm.Type).ToListAsync(cancellationToken);
    }

    public async Task<PaymentMethod?> GetDefaultByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive, cancellationToken);
    }

    public async Task<PaymentMethod> CreateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync(cancellationToken);
        return paymentMethod;
    }

    public async Task<PaymentMethod> UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        if (paymentMethod == null)
            throw new ArgumentNullException(nameof(paymentMethod));

        _context.PaymentMethods.Update(paymentMethod);
        await _context.SaveChangesAsync(cancellationToken);
        return paymentMethod;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await _context.PaymentMethods.FindAsync(id, cancellationToken);
        if (paymentMethod != null)
        {
            _context.PaymentMethods.Remove(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentMethods.AnyAsync(pm => pm.Id == id, cancellationToken);
    }

    public async Task SetDefaultAsync(Guid paymentMethodId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Use a transaction to ensure atomicity
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Unset any existing default for this user
            var currentDefaults = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var pm in currentDefaults)
            {
                pm.RemoveAsDefault();
            }

            // Set the specified payment method as default
            var newDefault = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.UserId == userId, cancellationToken);
            if (newDefault != null)
            {
                newDefault.SetAsDefault();
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}