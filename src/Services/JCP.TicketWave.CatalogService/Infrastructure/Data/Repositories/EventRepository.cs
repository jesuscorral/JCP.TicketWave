using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Data;
using JCP.TicketWave.CatalogService.Domain.Entities;

namespace JCP.TicketWave.CatalogService.Infrastructure.Data.Repositories;

public class EventRepository : IEventRepository
{
    private readonly CatalogDbContext _context;

    public EventRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Where(e => e.IsActive)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
    }

    public async Task<IEnumerable<Event>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.Events
            .Where(e => e.CategoryId == categoryId && e.IsActive)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetByVenueAsync(Guid venueId)
    {
        return await _context.Events
            .Where(e => e.VenueId == venueId && e.IsActive)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count = 10)
    {
        var now = DateTime.UtcNow;
        return await _context.Events
            .Where(e => e.IsActive && e.StartDateTime > now)
            .OrderBy(e => e.StartDateTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
    {
        return await _context.Events
            .Where(e => e.IsActive && 
                       (e.Title.Contains(searchTerm) || 
                        e.Description.Contains(searchTerm)))
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event> AddAsync(Event entity)
    {
        _context.Events.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Event> UpdateAsync(Event entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.Cancel("Deleted via repository"); // Use domain method
            await UpdateAsync(entity);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Events
            .AnyAsync(e => e.Id == id && e.IsActive);
    }
}