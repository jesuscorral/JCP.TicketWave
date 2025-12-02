using JCP.TicketWave.CatalogService.Domain.Enums;

namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = "default";
    public Guid CategoryId { get; private set; }
    public Guid VenueId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public EventStatus Status { get; private set; }
    public List<TicketTypeInfo> TicketTypes { get; private set; } = new();
    public List<string> Images { get; private set; } = new();
    public List<string> Tags { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Private constructor for serialization
    private Event() { }

    // Factory method for creating new events
    public static Event Create(
        Guid categoryId,
        Guid venueId,
        string title,
        string description,
        DateTime startDateTime,
        DateTime endDateTime,
        string? tenantId = null,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        
        if (startDateTime >= endDateTime)
            throw new ArgumentException("Start date must be before end date");
        
        if (startDateTime <= DateTime.UtcNow)
            throw new ArgumentException("Event cannot be scheduled in the past");

        return new Event
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            CategoryId = categoryId,
            VenueId = venueId,
            Title = title,
            Description = description,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Status = EventStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    // Business methods
    public void Publish(string? publishedBy = null)
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException($"Cannot publish event in {Status} status");

        if (StartDateTime <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot publish event scheduled in the past");

        if (!TicketTypes.Any())
            throw new InvalidOperationException("Cannot publish event without ticket types");

        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = publishedBy;
    }

    public void Cancel(string reason, string? cancelledBy = null)
    {
        if (Status == EventStatus.Cancelled)
            return;

        if (Status == EventStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed event");

        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = cancelledBy;
    }

    public void Complete(string? completedBy = null)
    {
        if (Status != EventStatus.Published)
            throw new InvalidOperationException($"Cannot complete event in {Status} status");

        if (DateTime.UtcNow < EndDateTime)
            throw new InvalidOperationException("Cannot complete event before end date");

        Status = EventStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = completedBy;
    }

    public void AddTicketType(TicketTypeInfo ticketType)
    {
        if (Status != EventStatus.Draft)
            throw new InvalidOperationException("Cannot modify ticket types after event is published");

        if (TicketTypes.Any(t => t.Name.Equals(ticketType.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Ticket type '{ticketType.Name}' already exists");

        TicketTypes.Add(ticketType);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required", nameof(imageUrl));

        if (!Images.Contains(imageUrl))
        {
            Images.Add(imageUrl);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag is required", nameof(tag));

        var normalizedTag = tag.ToLowerInvariant().Trim();
        if (!Tags.Contains(normalizedTag))
        {
            Tags.Add(normalizedTag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    // Computed properties for SQL Server queries
    public string SearchText => $"{Title} {Description} {string.Join(" ", Tags)}".ToLowerInvariant();
    public bool IsUpcoming => Status == EventStatus.Published && StartDateTime > DateTime.UtcNow;
    public bool IsActive => Status == EventStatus.Published;
    public string PartitionKey => $"{TenantId}#{CategoryId}";
}