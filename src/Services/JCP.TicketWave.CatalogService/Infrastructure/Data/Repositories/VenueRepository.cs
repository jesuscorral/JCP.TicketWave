using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using JCP.TicketWave.CatalogService.Domain.Entities;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Data.Models;
using System.Net;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly Container _container;
    private readonly ILogger<VenueRepository> _logger;

    public VenueRepository(CosmosDbService cosmosDbService, ILogger<VenueRepository> logger)
    {
        _container = cosmosDbService?.VenuesContainer ?? throw new ArgumentNullException(nameof(cosmosDbService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Venue?> GetByIdAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<VenueDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return MapToDomainEntity(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Venue {VenueId} not found with partition key {PartitionKey}", id, partitionKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venue {VenueId} with partition key {PartitionKey}", id, partitionKey);
            throw;
        }
    }

    public async Task<Venue?> GetByIdAsync(Guid id, string? cityId = null, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? "default";
        var city = cityId ?? "default";
        var partitionKey = $"{tenant}#{city}";
        
        return await GetByIdAsync(id, partitionKey, cancellationToken);
    }

    public async Task<IEnumerable<Venue>> GetByCityAsync(string cityId, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? "default";
        var partitionKey = $"{tenant}#{cityId}";
        
        try
        {
            var query = _container.GetItemLinqQueryable<VenueDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(v => v.TenantId == tenant && v.CityId == cityId)
            .OrderBy(v => v.Name);

            var results = new List<VenueDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Resource);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venues by city {CityId} for tenant {TenantId}", cityId, tenant);
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> GetByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Cross-partition query since we're querying by tenant across all cities
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.tenantId = @tenantId ORDER BY c.name")
                .WithParameter("@tenantId", tenantId);

            var results = new List<VenueDocument>();
            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                _logger.LogDebug("Cross-partition query for tenant consumed {RU} RUs", response.RequestCharge);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venues by tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.isActive = true ORDER BY c.name");

            var results = new List<VenueDocument>();
            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active venues");
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE CONTAINS(LOWER(c.name), @searchTerm) ORDER BY c.name")
                .WithParameter("@searchTerm", searchTerm.ToLowerInvariant());

            var results = new List<VenueDocument>();
            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                _logger.LogDebug("Search query consumed {RU} RUs", response.RequestCharge);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search venues with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> GetByCapacityRangeAsync(int minCapacity, int maxCapacity, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(@"
                SELECT * FROM c 
                WHERE c.capacity.totalCapacity >= @minCapacity 
                AND c.capacity.totalCapacity <= @maxCapacity 
                ORDER BY c.capacity.totalCapacity")
                .WithParameter("@minCapacity", minCapacity)
                .WithParameter("@maxCapacity", maxCapacity);

            var results = new List<VenueDocument>();
            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venues by capacity range {MinCapacity}-{MaxCapacity}", minCapacity, maxCapacity);
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> GetByLocationAsync(double latitude, double longitude, double radiusKm, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: This is a simplified distance calculation - for production use a proper geospatial query
            var latRange = radiusKm / 111.0; // Rough km per degree of latitude
            var lonRange = radiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0)); // Adjusted for longitude

            var query = new QueryDefinition(@"
                SELECT * FROM c 
                WHERE c.address.latitude >= @minLat 
                AND c.address.latitude <= @maxLat 
                AND c.address.longitude >= @minLon 
                AND c.address.longitude <= @maxLon")
                .WithParameter("@minLat", latitude - latRange)
                .WithParameter("@maxLat", latitude + latRange)
                .WithParameter("@minLon", longitude - lonRange)
                .WithParameter("@maxLon", longitude + lonRange);

            var results = new List<VenueDocument>();
            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venues by location");
            throw;
        }
    }

    public async Task<Venue> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(venue);
            
            var response = await _container.CreateItemAsync(
                document,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created venue {VenueId} consuming {RU} RUs", venue.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create venue {VenueId}", venue.Id);
            throw;
        }
    }

    public async Task<Venue> UpdateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = MapToDocument(venue);
            
            var response = await _container.ReplaceItemAsync(
                document,
                document.Id,
                new PartitionKey(document.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Updated venue {VenueId} consuming {RU} RUs", venue.Id, response.RequestCharge);
            return MapToDomainEntity(response.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update venue {VenueId}", venue.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.DeleteItemAsync<VenueDocument>(
                id.ToString(),
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted venue {VenueId} consuming {RU} RUs", id, response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Venue {VenueId} not found for deletion", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete venue {VenueId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _container.ReadItemAsync<VenueDocument>(
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

    public async Task<bool> NameExistsInCityAsync(string name, string cityId, string? tenantId = null, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? "default";
        var partitionKey = $"{tenant}#{cityId}";
        
        try
        {
            var query = _container.GetItemLinqQueryable<VenueDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .Where(v => v.TenantId == tenant && v.CityId == cityId && v.Name.ToLower() == name.ToLowerInvariant());

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.ToString());
            }

            using var feedIterator = query.ToFeedIterator();
            var response = await feedIterator.ReadNextAsync(cancellationToken);
            
            return response.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if venue name {Name} exists in city {CityId}", name, cityId);
            throw;
        }
    }

    public async Task<(IEnumerable<Venue> Items, string? ContinuationToken)> GetPagedAsync(
        int maxItemCount,
        string? continuationToken = null,
        string? cityId = null,
        string? tenantId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var tenant = tenantId ?? "default";
        
        try
        {
            var requestOptions = new QueryRequestOptions
            {
                MaxItemCount = maxItemCount
            };

            var queryText = "SELECT * FROM c WHERE c.tenantId = @tenantId";
            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@tenantId", tenant);

            if (!string.IsNullOrEmpty(cityId))
            {
                queryText += " AND c.cityId = @cityId";
                queryDefinition = queryDefinition.WithParameter("@cityId", cityId);
                
                // Use hierarchical partition key if filtering by city
                var partitionKey = $"{tenant}#{cityId}";
                requestOptions.PartitionKey = new PartitionKey(partitionKey);
            }

            if (isActive.HasValue)
            {
                queryText += " AND c.isActive = @isActive";
                queryDefinition = queryDefinition.WithParameter("@isActive", isActive.Value);
            }

            queryText += " ORDER BY c.name";
            queryDefinition = new QueryDefinition(queryText);
            
            // Re-add parameters after recreating QueryDefinition
            queryDefinition = queryDefinition.WithParameter("@tenantId", tenant);
            if (!string.IsNullOrEmpty(cityId))
            {
                queryDefinition = queryDefinition.WithParameter("@cityId", cityId);
            }
            if (isActive.HasValue)
            {
                queryDefinition = queryDefinition.WithParameter("@isActive", isActive.Value);
            }

            using var feedIterator = _container.GetItemQueryIterator<VenueDocument>(
                queryDefinition,
                continuationToken,
                requestOptions);

            var response = await feedIterator.ReadNextAsync(cancellationToken);
            var venues = response.Select(MapToDomainEntity);

            return (venues, response.ContinuationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get paged venues");
            throw;
        }
    }

    public async Task<IEnumerable<Venue>> GetVenuesByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _container.GetItemLinqQueryable<VenueDocument>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            })
            .OrderBy(v => v.Name);

            var results = new List<VenueDocument>();
            using var feedIterator = query.ToFeedIterator();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Resource);
            }

            return results.Select(MapToDomainEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get venues by partition {PartitionKey}", partitionKey);
            throw;
        }
    }

    public async Task<int> CountByPartitionAsync(string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c")
            {
            };

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
            _logger.LogError(ex, "Failed to count venues in partition {PartitionKey}", partitionKey);
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetCitiesByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new QueryDefinition(
                "SELECT DISTINCT VALUE c.cityId FROM c WHERE c.tenantId = @tenantId AND c.isActive = true ORDER BY c.cityId")
                .WithParameter("@tenantId", tenantId);

            var results = new List<string>();
            using var feedIterator = _container.GetItemQueryIterator<string>(query);
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cities for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private static Venue MapToDomainEntity(VenueDocument document)
    {
        var address = new VenueAddress(
            document.Address.Street,
            document.Address.City,
            document.Address.State,
            document.Address.Country,
            document.Address.PostalCode,
            document.Address.Latitude,
            document.Address.Longitude);

        var capacity = new VenueCapacity(
            document.Capacity.TotalCapacity,
            document.Capacity.HasSeatedSections,
            document.Capacity.HasStandingAreas)
        {
            SectionCapacities = document.Capacity.SectionCapacities
        };

        return Venue.Create(
            document.Name,
            document.Description,
            address,
            capacity,
            document.TenantId,
            document.CityId);
    }

    private static VenueDocument MapToDocument(Venue entity)
    {
        return new VenueDocument
        {
            Id = entity.Id.ToString(),
            TenantId = entity.TenantId,
            CityId = entity.CityId,
            Name = entity.Name,
            Description = entity.Description,
            Address = new VenueAddressDocument
            {
                Street = entity.Address.Street,
                City = entity.Address.City,
                State = entity.Address.State,
                Country = entity.Address.Country,
                PostalCode = entity.Address.PostalCode,
                Latitude = entity.Address.Latitude,
                Longitude = entity.Address.Longitude
            },
            Capacity = new VenueCapacityDocument
            {
                TotalCapacity = entity.Capacity.TotalCapacity,
                SectionCapacities = entity.Capacity.SectionCapacities,
                HasSeatedSections = entity.Capacity.HasSeatedSections,
                HasStandingAreas = entity.Capacity.HasStandingAreas
            },
            Amenities = entity.Amenities.ToList(),
            Images = entity.Images.ToList(),
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}