namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Event
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartDateTime { get; private set; }
    public DateTime EndDateTime { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid VenueId { get; private set; }
    public EventStatus Status { get; private set; }
    public int AvailableTickets { get; private set; }
    public decimal TicketPrice { get; private set; }
    public decimal? MaxPrice { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public string? ImageUrl { get; private set; }
    public string? ExternalUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public Category? Category { get; private set; }
    public Venue? Venue { get; private set; }

    // Private constructor for EF Core
    private Event() { }

    // Factory method for creating new events
    public static Event Create(
        string tenantId,
        string title,
        string? description,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid categoryId,
        Guid venueId,
        int availableTickets,
        decimal ticketPrice,
        string currency = "EUR",
        string? imageUrl = null,
        string? externalUrl = null)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title,
            Description = description,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            CategoryId = categoryId,
            VenueId = venueId,
            Status = EventStatus.Draft,
            AvailableTickets = availableTickets,
            TicketPrice = ticketPrice,
            Currency = currency,
            ImageUrl = imageUrl,
            ExternalUrl = externalUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void UpdateDetails(string title, string? description, string? imageUrl = null)
    {
        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(DateTime startDateTime, DateTime endDateTime)
    {
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePricing(decimal ticketPrice, decimal? maxPrice = null)
    {
        TicketPrice = ticketPrice;
        MaxPrice = maxPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvailableTickets(int availableTickets)
    {
        AvailableTickets = availableTickets;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status == EventStatus.Draft)
        {
            Status = EventStatus.Published;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Cancel()
    {
        if (Status != EventStatus.Completed)
        {
            Status = EventStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Complete()
    {
        if (Status == EventStatus.Published && DateTime.UtcNow > EndDateTime)
        {
            Status = EventStatus.Completed;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}