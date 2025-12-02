namespace JCP.TicketWave.CatalogService.Domain.Entities;

public class VenueCapacity
{
    public int TotalCapacity { get; set; }
    public Dictionary<string, int> SectionCapacities { get; set; } = new();
    public bool HasSeatedSections { get; set; }
    public bool HasStandingAreas { get; set; }

    public VenueCapacity() { }

    public VenueCapacity(int totalCapacity, bool hasSeatedSections = true, bool hasStandingAreas = false)
    {
        if (totalCapacity <= 0)
            throw new ArgumentException("Total capacity must be greater than zero", nameof(totalCapacity));

        TotalCapacity = totalCapacity;
        HasSeatedSections = hasSeatedSections;
        HasStandingAreas = hasStandingAreas;
    }

    public void AddSection(string sectionName, int capacity)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Section name is required", nameof(sectionName));
        
        if (capacity <= 0)
            throw new ArgumentException("Section capacity must be greater than zero", nameof(capacity));

        SectionCapacities[sectionName] = capacity;
    }

    public int GetSectionCapacity(string sectionName)
    {
        return SectionCapacities.GetValueOrDefault(sectionName, 0);
    }

    public bool IsValidConfiguration => SectionCapacities.Values.Sum() <= TotalCapacity;
}