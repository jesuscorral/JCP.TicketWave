using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    Task<Venue?> GetByIdAsync(Guid id, string? cityId = null, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> GetByCityAsync(string cityId, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> GetByCapacityRangeAsync(int minCapacity, int maxCapacity, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venue>> GetByLocationAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default);
    Task<Venue> CreateAsync(Venue venue, CancellationToken cancellationToken = default);
    Task<Venue> UpdateAsync(Venue venue, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    Task<bool> NameExistsInCityAsync(string name, string cityId, string? tenantId = null, Guid? excludeId = null, CancellationToken cancellationToken = default);
    
    // Pagination with HPK support (tenant + city)
    Task<(IEnumerable<Venue> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        string? cityId = null,
        string? tenantId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    
    // Cosmos DB specific operations
    Task<IEnumerable<Venue>> GetVenuesByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default);
    Task<int> CountByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetCitiesByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}