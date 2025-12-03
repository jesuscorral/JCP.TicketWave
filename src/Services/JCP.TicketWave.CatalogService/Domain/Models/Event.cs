using JCP.TicketWave.Shared.Infrastructure.Domain;
using JCP.TicketWave.CatalogService.Domain.Validators;
using JCP.TicketWave.CatalogService.Domain.Events;

namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Event : AggregateRoot
{
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
    
    // Navigation properties
    public Category? Category { get; private set; }
    public Venue? Venue { get; private set; }

    // Private constructor for EF Core
    private Event() : base() { }

    // Private constructor for factory method
    private Event(
        string tenantId,
        string title,
        string? description,
        DateTime startDateTime,
        DateTime endDateTime,
        Guid categoryId,
        Guid venueId,
        int availableTickets,
        decimal ticketPrice,
        string currency,
        string? imageUrl,
        string? externalUrl) : base()
    {
        TenantId = tenantId;
        Title = title;
        Description = description;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        CategoryId = categoryId;
        VenueId = venueId;
        Status = EventStatus.Draft;
        AvailableTickets = availableTickets;
        TicketPrice = ticketPrice;
        Currency = currency;
        ImageUrl = imageUrl;
        ExternalUrl = externalUrl;
    }

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
        // Create validation request
        var validationRequest = new CreateEventRequest(
            tenantId, title, description, startDateTime, endDateTime,
            categoryId, venueId, availableTickets, ticketPrice, null,
            currency, imageUrl, externalUrl);

        // Validate using FluentValidation
        var validator = new EventValidator();
        var validationResult = validator.Validate(validationRequest);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new DomainException($"Event validation failed: {errors}");
        }

        var eventInstance = new Event(
            tenantId,
            title,
            description,
            startDateTime,
            endDateTime,
            categoryId,
            venueId,
            availableTickets,
            ticketPrice,
            currency,
            imageUrl,
            externalUrl);

        // Add domain events
        eventInstance.AddDomainEvent(new EventCreatedDomainEvent(
            eventInstance.Id, title, description ?? string.Empty, startDateTime, endDateTime, 
            venueId, categoryId, ticketPrice, currency, availableTickets));

        return eventInstance;
    }

    // Business methods
    public void UpdateDetails(string title, string? description, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Event title is required");

        Title = title;
        Description = description;
        ImageUrl = imageUrl;
        MarkAsModified();

        // Add domain event
        AddDomainEvent(new EventUpdatedDomainEvent(Id, title, description ?? string.Empty, StartDateTime, EndDateTime, Currency, TicketPrice, DateTime.UtcNow));
    }

    public void UpdateSchedule(DateTime startDateTime, DateTime endDateTime)
    {
        if (startDateTime >= endDateTime)
            throw new DomainException("Start date must be before end date");

        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        MarkAsModified();
    }

    public void UpdatePricing(decimal ticketPrice, decimal? maxPrice = null)
    {
        if (ticketPrice < 0)
            throw new DomainException("Ticket price cannot be negative");

        TicketPrice = ticketPrice;
        MaxPrice = maxPrice;
        MarkAsModified();
    }

    public void UpdateAvailableTickets(int availableTickets)
    {
        if (availableTickets < 0)
            throw new DomainException("Available tickets cannot be negative");

        var wasSoldOut = AvailableTickets == 0;
        AvailableTickets = availableTickets;
        MarkAsModified();

        // Check if event just sold out
        if (!wasSoldOut && AvailableTickets == 0)
        {
            AddDomainEvent(new EventSoldOutDomainEvent(Id, Title, StartDateTime, AvailableTickets, DateTime.UtcNow));
        }
    }

    public void Publish()
    {
        if (Status == EventStatus.Draft)
        {
            Status = EventStatus.Published;
            MarkAsModified();

            // Add domain events
            AddDomainEvent(new EventPublishedDomainEvent(Id, Title, StartDateTime, EndDateTime, DateTime.UtcNow));
            AddDomainEvent(new EventCreatedIntegrationEvent(Id, Title, Description ?? string.Empty, StartDateTime, EndDateTime, VenueId, string.Empty, CategoryId, string.Empty, TicketPrice, Currency, AvailableTickets));
        }
    }

    public void Cancel(string reason = "Event cancelled by organizer")
    {
        if (Status != EventStatus.Completed)
        {
            Status = EventStatus.Cancelled;
            MarkAsModified();

            // Add domain events
            AddDomainEvent(new EventCancelledDomainEvent(Id, Title, StartDateTime, reason, DateTime.UtcNow));
            AddDomainEvent(new EventCancelledIntegrationEvent(Id, Title, StartDateTime, reason, DateTime.UtcNow));
        }
    }

    public void Complete()
    {
        if (Status == EventStatus.Published && DateTime.UtcNow > EndDateTime)
        {
            Status = EventStatus.Completed;
            MarkAsModified();
        }
    }
}