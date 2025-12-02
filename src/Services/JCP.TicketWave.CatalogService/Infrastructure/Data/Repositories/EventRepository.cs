using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using JCP.TicketWave.CatalogService.Domain.Entities;
using JCP.TicketWave.CatalogService.Domain.Enums;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Data.Models;
using System.Net;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;

public class EventRepository : IEventRepository
{
    private readonly Container _container;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(CosmosDbService cosmosDbService, ILogger<EventRepository> logger)
    {
        _container = cosmosDbService?.EventsContainer ?? throw new ArgumentNullException(nameof(cosmosDbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Event?> GetByIdAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<EventDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return MapToDomainEntity(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Event {EventId} not found in partition {PartitionKey}", id, partitionKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get event {EventId} from partition {PartitionKey}", id, partitionKey);
            throw;
        }
    }

    public async Task<Event?> GetByIdAsync(Guid id, Guid categoryId, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = $"{tenantId ?? "default"}#{categoryId}";
        return await GetByIdAsync(id, partitionKey, cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetByCategoryIdAsync(Guid categoryId, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = $"{tenantId ?? "default"}#{categoryId}";
        
        try
        {
            var query = _container.GetItemLinqQueryable<EventDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey),
                MaxItemCount = 100
            })
            .Where(e => e.TenantId == (tenantId ?? "default") && e.CategoryId == categoryId.ToString())
            .OrderByDescending(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                _logger.LogDebug("Query consumed {RU} RUs", response.RequestCharge);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cross-partition query - use sparingly
            var query = _container.GetItemLinqQueryable<EventDocument>()
                .Where(e => e.VenueId == venueId.ToString() && e.IsActive)
                .OrderByDescending(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                _logger.LogDebug("Cross-partition query consumed {RU} RUs", response.RequestCharge);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events for venue {VenueId}", venueId);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetByStatusAsync(EventStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cross-partition query with status filter
            var query = _container.GetItemLinqQueryable<EventDocument>()
                .Where(e => e.Status == status.ToString())
                .OrderByDescending(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events by status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int maxCount = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var query = _container.GetItemLinqQueryable<EventDocument>(requestOptions: new QueryRequestOptions
            {
                MaxItemCount = maxCount
            })
            .Where(e => e.IsActive && e.StartDateTime > now)
            .OrderBy(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults && results.Count < maxCount)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Take(maxCount - results.Count));
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get upcoming events");
            throw;
        }
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSearch = searchTerm.ToLowerInvariant();
            var query = _container.GetItemLinqQueryable<EventDocument>()
                .Where(e => e.IsActive && e.SearchText.Contains(normalizedSearch))
                .OrderByDescending(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search events with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _container.GetItemLinqQueryable<EventDocument>()
                .Where(e => e.StartDateTime >= startDate && e.StartDateTime <= endDate)
                .OrderBy(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events by date range {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<Event> CreateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(eventEntity);
            
            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created event {EventId} consuming {RU} RUs", eventEntity.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create event {EventId}", eventEntity.Id);
            throw;
        }
    }

    public async Task<Event> UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(eventEntity);
            
            var response = await _container.ReplaceItemAsync(
                document,
                document.Id,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Updated event {EventId} consuming {RU} RUs", eventEntity.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update event {EventId}", eventEntity.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.DeleteItemAsync<EventDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted event {EventId} consuming {RU} RUs", id, response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Event {EventId} not found for deletion", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete event {EventId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReadItemAsync<EventDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<(IEnumerable<Event> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        Guid? categoryId = null,
        string? tenantId = null,
        EventStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = maxItemCount
            };

            // Use partition key if category is specified
            if (categoryId.HasValue && !string.IsNullOrEmpty(tenantId))
            {
                requestOptions.PartitionKey = new PartitionKey($"{tenantId}#{categoryId}");
            }

            var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE 1=1");
            
            if (categoryId.HasValue)
                queryDefinition = queryDefinition.WithParameter("@categoryId", categoryId.ToString());
            
            if (!string.IsNullOrEmpty(tenantId))
                queryDefinition = queryDefinition.WithParameter("@tenantId", tenantId);
                
            if (status.HasValue)
                queryDefinition = queryDefinition.WithParameter("@status", status.ToString());

            using var feedIterator = _container.GetItemQueryIterator<EventDocument>(
                queryDefinition,
                continuationToken,
                requestOptions);

            var response = await feedIterator.ReadNextAsync(cancellationToken);
            var events = response.Select(MapToDomainEntity);

            return (events, response.ContinuationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged events");
            throw;
        }
    }

    public async Task<IEnumerable<Event>> GetEventsByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _container.GetItemLinqQueryable<EventDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .OrderByDescending(e => e.StartDateTime);

            var results = new List<EventDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get events by partition {PartitionKey}", partitionKey);
            throw;
        }
    }

    public async Task<int> CountByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c")
                .WithParameter("@partitionKey", partitionKey);

            using var feedIterator = _container.GetItemQueryIterator<int>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });

            var response = await feedIterator.ReadNextAsync(cancellationToken);
            return response.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count events by partition {PartitionKey}", partitionKey);
            throw;
        }
    }

    private static Event MapToDomainEntity(EventDocument document)
    {
        var eventEntity = Event.Create(
            Guid.Parse(document.CategoryId),
            Guid.Parse(document.VenueId),
            document.Title,
            document.Description,
            document.StartDateTime,
            document.EndDateTime,
            document.TenantId,
            document.CreatedBy);

        // Set private fields using reflection or create a method in the domain entity
        // For now, we'll create a new instance and copy properties
        var ticketTypes = document.TicketTypes.Select(tt => new TicketTypeInfo(
            tt.Name,
            tt.Price,
            tt.TotalAvailable,
            tt.Description
        )).ToList();

        foreach (var ticketType in ticketTypes)
        {
            eventEntity.AddTicketType(ticketType);
        }

        foreach (var image in document.Images)
        {
            eventEntity.AddImage(image);
        }

        foreach (var tag in document.Tags)
        {
            eventEntity.AddTag(tag);
        }

        return eventEntity;
    }

    private static EventDocument MapToDocument(Event entity)
    {
        return new EventDocument
        {
            Id = entity.Id.ToString(),
            TenantId = entity.TenantId,
            CategoryId = entity.CategoryId.ToString(),
            VenueId = entity.VenueId.ToString(),
            Title = entity.Title,
            Description = entity.Description,
            StartDateTime = entity.StartDateTime,
            EndDateTime = entity.EndDateTime,
            Status = entity.Status.ToString(),
            TicketTypes = entity.TicketTypes.Select(tt => new TicketTypeDocument
            {
                Name = tt.Name,
                Price = tt.Price,
                TotalAvailable = tt.TotalAvailable,
                AvailableCount = tt.AvailableCount,
                Description = tt.Description,
                Attributes = tt.Attributes
            }).ToList(),
            Images = entity.Images,
            Tags = entity.Tags,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };
    }
}