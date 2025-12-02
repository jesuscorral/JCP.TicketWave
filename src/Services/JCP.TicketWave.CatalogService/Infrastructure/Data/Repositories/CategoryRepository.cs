using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using JCP.TicketWave.CatalogService.Domain.Entities;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Data.Models;
using System.Net;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly Container _container;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(CosmosDbService cosmosDbService, ILogger<CategoryRepository> logger)
    {
        _container = cosmosDbService?.CategoriesContainer ?? throw new ArgumentNullException(nameof(cosmosDbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Category?> GetByIdAsync(Guid id, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var response = await _container.ReadItemAsync<CategoryDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return MapToDomainEntity(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Category {CategoryId} not found in tenant {TenantId}", id, partitionKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get category {CategoryId} from tenant {TenantId}", id, partitionKey);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = _container.GetItemLinqQueryable<CategoryDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(c => c.TenantId == partitionKey)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name);

            var results = new List<CategoryDocument>();
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
            _logger.LogError(ex, "Failed to get all categories for tenant {TenantId}", partitionKey);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetActiveAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = _container.GetItemLinqQueryable<CategoryDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(c => c.TenantId == partitionKey && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name);

            var results = new List<CategoryDocument>();
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
            _logger.LogError(ex, "Failed to get active categories for tenant {TenantId}", partitionKey);
            throw;
        }
    }

    public async Task<IEnumerable<Category>> GetByDisplayOrderAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = _container.GetItemLinqQueryable<CategoryDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(c => c.TenantId == partitionKey)
            .OrderBy(c => c.DisplayOrder);

            var results = new List<CategoryDocument>();
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
            _logger.LogError(ex, "Failed to get categories by display order for tenant {TenantId}", partitionKey);
            throw;
        }
    }

    public async Task<Category?> GetByNameAsync(string name, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = _container.GetItemLinqQueryable<CategoryDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(c => c.TenantId == partitionKey && c.Name == name);

            using var feedIterator = query.ToFeedIterator();
            var response = await feedIterator.ReadNextAsync(cancellationToken);
            
            return response.FirstOrDefault() != null ? MapToDomainEntity(response.First()) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get category by name {Name} for tenant {TenantId}", name, partitionKey);
            throw;
        }
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(category);
            
            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created category {CategoryId} consuming {RU} RUs", category.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task<Category> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(category);
            
            var response = await _container.ReplaceItemAsync(
                document,
                document.Id,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Updated category {CategoryId} consuming {RU} RUs", category.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update category {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.DeleteItemAsync<CategoryDocument>(
                id.ToString(),
                new PartitionKey(tenantId),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted category {CategoryId} consuming {RU} RUs", id, response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Category {CategoryId} not found for deletion", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete category {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReadItemAsync<CategoryDocument>(
                id.ToString(),
                new PartitionKey(tenantId),
                cancellationToken: cancellationToken);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<bool> NameExistsAsync(string name, string? tenantId = null, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = _container.GetItemLinqQueryable<CategoryDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(c => c.TenantId == partitionKey && c.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.ToString());
            }

            using var feedIterator = query.ToFeedIterator();
            var response = await feedIterator.ReadNextAsync(cancellationToken);
            
            return response.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if category name {Name} exists", name);
            throw;
        }
    }

    public async Task<int> GetNextDisplayOrderAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var query = new QueryDefinition(
                "SELECT VALUE MAX(c.displayOrder) FROM c WHERE c.tenantId = @tenantId")
                .WithParameter("@tenantId", partitionKey);

            using var feedIterator = _container.GetItemQueryIterator<int?>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });

            var response = await feedIterator.ReadNextAsync(cancellationToken);
            var maxOrder = response.FirstOrDefault();
            
            return (maxOrder ?? 0) + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next display order for tenant {TenantId}", partitionKey);
            throw;
        }
    }

    public async Task<(IEnumerable<Category> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        string? tenantId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var partitionKey = tenantId ?? "default";
        
        try
        {
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = maxItemCount,
                PartitionKey = new PartitionKey(partitionKey)
            };

            var queryText = "SELECT * FROM c WHERE c.tenantId = @tenantId";
            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@tenantId", partitionKey);

            if (isActive.HasValue)
            {
                queryText += " AND c.isActive = @isActive";
                queryDefinition = queryDefinition.WithParameter("@isActive", isActive.Value);
            }

            queryText += " ORDER BY c.displayOrder, c.name";
            queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@tenantId", partitionKey);
                
            if (isActive.HasValue)
            {
                queryDefinition = queryDefinition.WithParameter("@isActive", isActive.Value);
            }

            using var feedIterator = _container.GetItemQueryIterator<CategoryDocument>(
                queryDefinition,
                continuationToken,
                requestOptions);

            var response = await feedIterator.ReadNextAsync(cancellationToken);
            var categories = response.Select(MapToDomainEntity);

            return (categories, response.ContinuationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged categories");
            throw;
        }
    }

    private static Category MapToDomainEntity(CategoryDocument document)
    {
        return Category.Create(
            document.Name,
            document.Description,
            document.TenantId,
            document.IconUrl,
            document.Color);
    }

    private static CategoryDocument MapToDocument(Category entity)
    {
        return new CategoryDocument
        {
            Id = entity.Id.ToString(),
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            IconUrl = entity.IconUrl,
            Color = entity.Color,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}