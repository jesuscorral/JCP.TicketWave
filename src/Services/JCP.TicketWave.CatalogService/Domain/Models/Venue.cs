using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Venue : BaseEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string CityId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string? PostalCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public int Capacity { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    // Navigation properties
    public ICollection<Event> Events { get; private set; } = new List<Event>();

    // Private constructor for EF Core
    private Venue() : base() { }

    // Private constructor for factory method
    private Venue(
        string tenantId,
        string cityId,
        string name,
        string address,
        int capacity,
        string? description,
        string? postalCode,
        string? phoneNumber,
        string? email,
        string? website,
        double? latitude,
        double? longitude,
        string? imageUrl) : base()
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new DomainException("Tenant ID is required");
        
        if (string.IsNullOrWhiteSpace(cityId))
            throw new DomainException("City ID is required");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Venue name is required");
        
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Venue address is required");
        
        if (capacity <= 0)
            throw new DomainException("Venue capacity must be greater than zero");

        TenantId = tenantId;
        CityId = cityId;
        Name = name;
        Description = description;
        Address = address;
        PostalCode = postalCode;
        PhoneNumber = phoneNumber;
        Email = email;
        Website = website;
        Capacity = capacity;
        Latitude = latitude;
        Longitude = longitude;
        ImageUrl = imageUrl;
        IsActive = true;
    }

    // Factory method for creating new venues
    public static Venue Create(
        string tenantId,
        string cityId,
        string name,
        string address,
        int capacity,
        string? description = null,
        string? postalCode = null,
        string? phoneNumber = null,
        string? email = null,
        string? website = null,
        double? latitude = null,
        double? longitude = null,
        string? imageUrl = null)
    {
        return new Venue(
            tenantId,
            cityId,
            name,
            address,
            capacity,
            description,
            postalCode,
            phoneNumber,
            email,
            website,
            latitude,
            longitude,
            imageUrl);
    }

    // Business methods
    public void UpdateDetails(
        string name,
        string? description = null,
        string? phoneNumber = null,
        string? email = null,
        string? website = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Venue name is required");

        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        Website = website;
        ImageUrl = imageUrl;
        UpdateTimestamp();
    }

    public void UpdateAddress(string address, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Venue address is required");

        Address = address;
        PostalCode = postalCode;
        UpdateTimestamp();
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        UpdateTimestamp();
    }

    public void UpdateCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new DomainException("Venue capacity must be greater than zero");

        Capacity = capacity;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }
}