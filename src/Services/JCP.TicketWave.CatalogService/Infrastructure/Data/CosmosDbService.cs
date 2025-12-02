using Microsoft.Azure.Cosmos;
using System.Collections.ObjectModel;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data;

public static class CosmosDbConfiguration
{
    public const string DatabaseName = "TicketWave";
    public const string EventsContainerName = "events";
    public const string CategoriesContainerName = "categories";
    public const string VenuesContainerName = "venues";

    // Hierarchical Partition Keys for better scalability and query performance
    public static readonly string[] EventsPartitionKeyPaths = { "/tenantId", "/categoryId" };
    public static readonly string[] CategoriesPartitionKeyPaths = { "/tenantId" };
    public static readonly string[] VenuesPartitionKeyPaths = { "/tenantId", "/cityId" };

    // Container configurations
    public static class Events
    {
        public const int DefaultThroughput = 400; // Autoscale min
        public const int MaxThroughput = 4000; // Autoscale max
        public static readonly string[] PartitionKeyPaths = EventsPartitionKeyPaths;
        public const string IndexingPolicy = @"
        {
            ""indexingMode"": ""consistent"",
            ""automatic"": true,
            ""includedPaths"": [
                {
                    ""path"": ""/*""
                }
            ],
            ""excludedPaths"": [
                {
                    ""path"": ""/images/*""
                },
                {
                    ""path"": ""/description/?""
                }
            ],
            ""compositeIndexes"": [
                [
                    {
                        ""path"": ""/tenantId"",
                        ""order"": ""ascending""
                    },
                    {
                        ""path"": ""/categoryId"",
                        ""order"": ""ascending""
                    },
                    {
                        ""path"": ""/startDateTime"",
                        ""order"": ""descending""
                    }
                ],
                [
                    {
                        ""path"": ""/status"",
                        ""order"": ""ascending""
                    },
                    {
                        ""path"": ""/startDateTime"",
                        ""order"": ""ascending""
                    }
                ]
            ]
        }";
    }

    public static class Categories
    {
        public const int DefaultThroughput = 400;
        public const int MaxThroughput = 1000;
        public static readonly string[] PartitionKeyPaths = CategoriesPartitionKeyPaths;
        public const string IndexingPolicy = @"
        {
            ""indexingMode"": ""consistent"",
            ""automatic"": true,
            ""includedPaths"": [
                {
                    ""path"": ""/*""
                }
            ],
            ""excludedPaths"": [
                {
                    ""path"": ""/description/?""
                }
            ]
        }";
    }

    public static class Venues
    {
        public const int DefaultThroughput = 400;
        public const int MaxThroughput = 2000;
        public static readonly string[] PartitionKeyPaths = VenuesPartitionKeyPaths;
        public const string IndexingPolicy = @"
        {
            ""indexingMode"": ""consistent"",
            ""automatic"": true,
            ""includedPaths"": [
                {
                    ""path"": ""/*""
                }
            ],
            ""excludedPaths"": [
                {
                    ""path"": ""/description/?""
                },
                {
                    ""path"": ""/images/*""
                },
                {
                    ""path"": ""/amenities/*""
                }
            ],
            ""spatialIndexes"": [
                {
                    ""path"": ""/address/latitude/?"",
                    ""types"": [""Point""]
                },
                {
                    ""path"": ""/address/longitude/?"",
                    ""types"": [""Point""]
                }
            ]
        }";
    }
}

public class CosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Database _database;
    private readonly Container _eventsContainer;
    private readonly Container _categoriesContainer;
    private readonly Container _venuesContainer;
    private readonly ILogger<CosmosDbService> _logger;

    public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _database = _cosmosClient.GetDatabase(CosmosDbConfiguration.DatabaseName);
        _eventsContainer = _database.GetContainer(CosmosDbConfiguration.EventsContainerName);
        _categoriesContainer = _database.GetContainer(CosmosDbConfiguration.CategoriesContainerName);
        _venuesContainer = _database.GetContainer(CosmosDbConfiguration.VenuesContainerName);
    }

    public Container EventsContainer => _eventsContainer;
    public Container CategoriesContainer => _categoriesContainer;
    public Container VenuesContainer => _venuesContainer;
    public Database Database => _database;

    // Initialize database and containers
    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Cosmos DB database and containers...");

            // Create database with shared throughput for cost optimization
            var databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(
                id: CosmosDbConfiguration.DatabaseName,
                throughput: null); // Use container-level throughput instead

            _logger.LogInformation("Database '{DatabaseName}' ready", CosmosDbConfiguration.DatabaseName);

            // Create containers with optimized configurations
            await CreateEventContainerAsync();
            await CreateCategoryContainerAsync();
            await CreateVenueContainerAsync();

            _logger.LogInformation("Cosmos DB initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Cosmos DB");
            throw;
        }
    }

    private async Task CreateEventContainerAsync()
    {
        var containerProperties = new ContainerProperties
        {
            Id = CosmosDbConfiguration.EventsContainerName,
            PartitionKeyPath = "/partitionKey" // Use single composite key like "tenant#category"
        };

        // Configure index policy for optimized queries
        containerProperties.IndexingPolicy = new IndexingPolicy
        {
            Automatic = true,
            IndexingMode = IndexingMode.Consistent
        };
        
        // Add composite index for common query patterns
        containerProperties.IndexingPolicy.CompositeIndexes.Add(new Collection<CompositePath>
        {
            new() { Path = "/tenantId", Order = CompositePathSortOrder.Ascending },
            new() { Path = "/startDate", Order = CompositePathSortOrder.Ascending }
        });

        // Configure autoscale throughput
        var throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(
            CosmosDbConfiguration.Events.MaxThroughput);

        await _database.CreateContainerIfNotExistsAsync(
            containerProperties,
            throughputProperties);

        _logger.LogInformation("Events container created with partition key and autoscale throughput");
    }

    private async Task CreateCategoryContainerAsync()
    {
        var containerProperties = new ContainerProperties
        {
            Id = CosmosDbConfiguration.CategoriesContainerName,
            PartitionKeyPath = "/tenantId"
        };

        var throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(
            CosmosDbConfiguration.Categories.MaxThroughput);

        await _database.CreateContainerIfNotExistsAsync(
            containerProperties,
            throughputProperties);

        _logger.LogInformation("Categories container created");
    }

    private async Task CreateVenueContainerAsync()
    {
        var containerProperties = new ContainerProperties
        {
            Id = CosmosDbConfiguration.VenuesContainerName,
            PartitionKeyPath = "/partitionKey" // Use single composite key like "tenant#city"
        };

        // Configure index policy for location and capacity queries
        containerProperties.IndexingPolicy = new IndexingPolicy
        {
            Automatic = true,
            IndexingMode = IndexingMode.Consistent
        };

        // Add spatial indexing for location-based queries
        containerProperties.IndexingPolicy.SpatialIndexes.Add(
            new SpatialPath
            {
                Path = "/address/coordinates/*",
                SpatialTypes = { SpatialType.Point }
            });

        var throughputProperties = ThroughputProperties.CreateAutoscaleThroughput(
            CosmosDbConfiguration.Venues.MaxThroughput);

        await _database.CreateContainerIfNotExistsAsync(
            containerProperties,
            throughputProperties);

        _logger.LogInformation("Venues container created with spatial indexing");
    }

    // Health check method
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _database.ReadAsync(cancellationToken: cancellationToken);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cosmos DB health check failed");
            return false;
        }
    }

    // Get diagnostics for monitoring and debugging
    public string GetDiagnostics()
    {
        return $"Database: {_database.Id}, Client Endpoint: {_cosmosClient.Endpoint}";
    }
}