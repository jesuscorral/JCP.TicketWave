using System.Text.Json.Serialization;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Models;

public class VenueDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = "default";

    [JsonPropertyName("cityId")]
    public string CityId { get; set; } = "default";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public VenueAddressDocument Address { get; set; } = new();

    [JsonPropertyName("capacity")]
    public VenueCapacityDocument Capacity { get; set; } = new();

    [JsonPropertyName("amenities")]
    public List<string> Amenities { get; set; } = new();

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    // Partition key for HPK queries
    [JsonPropertyName("partitionKey")]
    public string PartitionKey => $"{TenantId}#{CityId}";
}

public class VenueAddressDocument
{
    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }
}

public class VenueCapacityDocument
{
    [JsonPropertyName("totalCapacity")]
    public int TotalCapacity { get; set; }

    [JsonPropertyName("sectionCapacities")]
    public Dictionary<string, int> SectionCapacities { get; set; } = new();

    [JsonPropertyName("hasSeatedSections")]
    public bool HasSeatedSections { get; set; }

    [JsonPropertyName("hasStandingAreas")]
    public bool HasStandingAreas { get; set; }
}