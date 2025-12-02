using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Domain.Interfaces;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Event>> GetByVenueAsync(Guid venueId);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count = 10);
    Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    Task<Event> AddAsync(Event entity);
    Task<Event> UpdateAsync(Event entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}