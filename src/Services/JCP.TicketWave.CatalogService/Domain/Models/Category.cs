namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; } = 0;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public ICollection<Event> Events { get; private set; } = new List<Event>();

    // Private constructor for EF Core
    private Category() { }

    // Factory method for creating new categories
    public static Category Create(
        string tenantId,
        string name,
        string? description = null,
        string? iconUrl = null,
        string? color = null,
        int displayOrder = 0)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Description = description,
            IconUrl = iconUrl,
            Color = color,
            IsActive = true,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void UpdateDetails(string name, string? description = null, string? iconUrl = null, string? color = null)
    {
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
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