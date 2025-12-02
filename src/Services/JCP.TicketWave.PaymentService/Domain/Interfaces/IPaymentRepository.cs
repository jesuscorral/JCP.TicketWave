using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<Payment?> GetByExternalIdAsync(string externalPaymentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> BookingHasPaymentAsync(Guid bookingId, CancellationToken cancellationToken = default);
    
    // Pagination
    Task<(IEnumerable<Payment> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? tenantId = null,
        PaymentStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
    
    // Analytics
    Task<decimal> GetTotalAmountByTenantAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<PaymentStatus, int>> GetPaymentStatisticsAsync(string? tenantId = null, CancellationToken cancellationToken = default);
}