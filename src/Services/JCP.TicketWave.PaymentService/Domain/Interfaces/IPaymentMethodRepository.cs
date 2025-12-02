using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Domain.Interfaces;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetByExternalIdAsync(string externalMethodId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentMethod>> GetByTenantAsync(string tenantId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<PaymentMethod?> GetDefaultByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentMethod> CreateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task<PaymentMethod> UpdateAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(Guid paymentMethodId, Guid userId, CancellationToken cancellationToken = default);
}