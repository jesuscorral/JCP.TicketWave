using System.Text.Json.Serialization;
using JCP.TicketWave.CatalogService.Domain.Enums;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Models;

public class EventDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = "default";

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("venueId")]
    public string VenueId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("startDateTime")]
    public DateTime StartDateTime { get; set; }

    [JsonPropertyName("endDateTime")]
    public DateTime EndDateTime { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("ticketTypes")]
    public List<TicketTypeDocument> TicketTypes { get; set; } = new();

    [JsonPropertyName("images")]
    public List<string> Images { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updatedBy")]
    public string? UpdatedBy { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }

    // Computed properties for efficient queries
    [JsonPropertyName("searchText")]
    public string SearchText => $"{Title} {Description} {string.Join(" ", Tags)}".ToLowerInvariant();

    [JsonPropertyName("partitionKey")]
    public string PartitionKey => $"{TenantId}#{CategoryId}";

    [JsonPropertyName("isActive")]
    public bool IsActive => Status == EventStatus.Published.ToString();

    [JsonPropertyName("isUpcoming")]
    public bool IsUpcoming => IsActive && StartDateTime > DateTime.UtcNow;
}

public class TicketTypeDocument
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("totalAvailable")]
    public int TotalAvailable { get; set; }

    [JsonPropertyName("availableCount")]
    public int AvailableCount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, object> Attributes { get; set; } = new();
}