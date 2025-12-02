using JCP.TicketWave.PaymentService.Domain.Entities;

namespace JCP.TicketWave.PaymentService.Domain.Interfaces;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Refund?> GetByExternalIdAsync(string externalRefundId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Refund>> GetByPaymentIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Refund>> GetByStatusAsync(RefundStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Refund>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Refund> CreateAsync(Refund refund, CancellationToken cancellationToken = default);
    Task<Refund> UpdateAsync(Refund refund, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundAmountForPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    
    // Pagination
    Task<(IEnumerable<Refund> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        RefundStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}