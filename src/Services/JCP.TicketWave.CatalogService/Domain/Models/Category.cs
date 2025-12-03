using JCP.TicketWave.Shared.Infrastructure.Domain;

namespace JCP.TicketWave.CatalogService.Domain.Models;

public class Category : BaseEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }
    public string? Color { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; } = 0;
    
    // Navigation properties
    public ICollection<Event> Events { get; private set; } = new List<Event>();

    // Private constructor for EF Core
    private Category() : base() { }

    // Private constructor for factory method
    private Category(
        string tenantId,
        string name,
        string? description,
        string? iconUrl,
        string? color,
        int displayOrder) : base()
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new DomainException("Tenant ID is required");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required");

        TenantId = tenantId;
        Name = name;
        Description = description;
        IconUrl = iconUrl;
        Color = color;
        IsActive = true;
        DisplayOrder = displayOrder;
    }

    // Factory method for creating new categories
    public static Category Create(
        string tenantId,
        string name,
        string? description = null,
        string? iconUrl = null,
        string? color = null,
        int displayOrder = 0)
    {
        return new Category(tenantId, name, description, iconUrl, color, displayOrder);
    }

    // Business methods
    public void UpdateDetails(string name, string? description = null, string? iconUrl = null, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required");

        Name = name;
        Description = description;
        IconUrl = iconUrl;
        Color = color;
        UpdateTimestamp();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
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