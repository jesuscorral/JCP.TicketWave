using JCP.TicketWave.CatalogService.Domain.Entities;
using JCP.TicketWave.CatalogService.Domain.Enums;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid id, Guid categoryId, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByCategoryIdAsync(Guid categoryId, string? tenantId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int maxCount = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default);
    
    // Pagination support with cross-partition queries minimized
    Task<(IEnumerable<Event> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        Guid? categoryId = null,
        string? tenantId = null,
        EventStatus? status = null,
        CancellationToken cancellationToken = default);
    
    // Cosmos DB specific operations
    Task<IEnumerable<Event>> GetEventsByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default);
    Task<int> CountByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default);
}