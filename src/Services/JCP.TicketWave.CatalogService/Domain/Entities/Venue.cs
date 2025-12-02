namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class Venue
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = "default";
    public string CityId { get; private set; } = "default";
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public VenueAddress Address { get; private set; } = new();
    public VenueCapacity Capacity { get; private set; } = new();
    public List<string> Amenities { get; private set; } = new();
    public List<string> Images { get; private set; } = new();
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for serialization
    private Venue() { }

    // Factory method
    public static Venue Create(
        string name,
        string description,
        VenueAddress address,
        VenueCapacity capacity,
        string? tenantId = null,
        string? cityId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        
        if (address == null)
            throw new ArgumentNullException(nameof(address));
        
        if (capacity == null)
            throw new ArgumentNullException(nameof(capacity));

        return new Venue
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            CityId = cityId ?? "default",
            Name = name,
            Description = description,
            Address = address,
            Capacity = capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, VenueAddress address, VenueCapacity capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name;
        Description = description;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Capacity = capacity ?? throw new ArgumentNullException(nameof(capacity));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAmenity(string amenity)
    {
        if (string.IsNullOrWhiteSpace(amenity))
            throw new ArgumentException("Amenity is required", nameof(amenity));

        var normalizedAmenity = amenity.Trim();
        if (!Amenities.Contains(normalizedAmenity))
        {
            Amenities.Add(normalizedAmenity);
            UpdatedAt = DateTime.UtcNow;
        }
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

    // Partition key for SQL Server (HPK)
    public string PartitionKey => $"{TenantId}#{CityId}";
}