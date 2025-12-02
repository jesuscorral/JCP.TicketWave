namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = "default";
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? IconUrl { get; private set; }
    public string Color { get; private set; } = "#007ACC";
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for serialization
    private Category() { }

    // Factory method
    public static Category Create(
        string name,
        string description,
        string? tenantId = null,
        string? iconUrl = null,
        string color = "#007ACC")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new Category
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId ?? "default",
            Name = name,
            Description = description,
            IconUrl = iconUrl,
            Color = color,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description, string? iconUrl = null, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name;
        Description = description;
        IconUrl = iconUrl;
        if (!string.IsNullOrWhiteSpace(color))
            Color = color;
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

    public void SetDisplayOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Display order cannot be negative", nameof(order));
        
        DisplayOrder = order;
        UpdatedAt = DateTime.UtcNow;
    }

    // Partition key for SQL Server
    public string PartitionKey => TenantId;
}