using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetAllAsync(string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetActiveAsync(string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetByDisplayOrderAsync(string? tenantId = null, CancellationToken cancellationToken = default);
    Task<Category?> GetByNameAsync(string name, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, string? tenantId = null, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> GetNextDisplayOrderAsync(string? tenantId = null, CancellationToken cancellationToken = default);
    
    // Pagination within tenant (single partition)
    Task<(IEnumerable<Category> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        string? tenantId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}