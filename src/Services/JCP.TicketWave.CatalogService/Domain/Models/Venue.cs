namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Venue
{
    public Guid Id { get; private set; } = Guid.NewGuid();
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
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public ICollection<Event> Events { get; private set; } = new List<Event>();

    // Private constructor for EF Core
    private Venue() { }

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
        return new Venue
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CityId = cityId,
            Name = name,
            Description = description,
            Address = address,
            PostalCode = postalCode,
            PhoneNumber = phoneNumber,
            Email = email,
            Website = website,
            Capacity = capacity,
            Latitude = latitude,
            Longitude = longitude,
            ImageUrl = imageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
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
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Email = email;
        Website = website;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(string address, string? postalCode = null)
    {
        Address = address;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCapacity(int capacity)
    {
        Capacity = capacity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}