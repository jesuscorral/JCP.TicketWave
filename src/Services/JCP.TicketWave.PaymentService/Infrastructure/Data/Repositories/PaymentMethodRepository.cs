using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.PaymentService.Domain.Entities;
using JCP.TicketWave.PaymentService.Domain.Interfaces;
using JCP.TicketWave.PaymentService.Infrastructure.Data;

namespace JCP.TicketWave.PaymentService.Infrastructure.Data.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<PaymentMethodRepository> _logger;

    public PaymentMethodRepository(PaymentDbContext context, ILogger<PaymentMethodRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment method {PaymentMethodId}", id);
            throw;
        }
    }

    public async Task<PaymentMethod?> GetByExternalIdAsync(string externalMethodId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.ExternalMethodId == externalMethodId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment method with external ID {ExternalMethodId}", externalMethodId);
            throw;
        }
    }

    public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsActive)
                .OrderByDescending(pm => pm.IsDefault)
                .ThenByDescending(pm => pm.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment methods for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<PaymentMethod>> GetByTenantAsync(string tenantId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.PaymentMethods
                .Where(pm => pm.TenantId == tenantId);

            if (activeOnly)
                query = query.Where(pm => pm.IsActive);

            return await query
                .OrderByDescending(pm => pm.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payment methods for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<PaymentMethod?> GetDefaultByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.UserId == userId && pm.IsDefault && pm.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get default payment method for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentMethod> CreateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created payment method {PaymentMethodId} for user {UserId}", 
                paymentMethod.Id, paymentMethod.UserId);
            return paymentMethod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment method {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    public async Task<PaymentMethod> UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.PaymentMethods.Update(paymentMethod);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated payment method {PaymentMethodId}", paymentMethod.Id);
            return paymentMethod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update payment method {PaymentMethodId}", paymentMethod.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync([id], cancellationToken);
            if (paymentMethod != null)
            {
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Deleted payment method {PaymentMethodId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete payment method {PaymentMethodId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PaymentMethods.AnyAsync(pm => pm.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if payment method {PaymentMethodId} exists", id);
            throw;
        }
    }

    public async Task SetDefaultAsync(Guid paymentMethodId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Remove default from all user's payment methods
            var userPaymentMethods = await _context.PaymentMethods
                .Where(pm => pm.UserId == userId && pm.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var method in userPaymentMethods)
            {
                method.RemoveAsDefault();
            }

            // Set the specified method as default
            var targetMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.UserId == userId, cancellationToken);

            if (targetMethod == null)
                throw new InvalidOperationException($"Payment method {paymentMethodId} not found for user {userId}");

            targetMethod.SetAsDefault();

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Set payment method {PaymentMethodId} as default for user {UserId}", paymentMethodId, userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to set payment method {PaymentMethodId} as default for user {UserId}", paymentMethodId, userId);
            throw;
        }
    }
}