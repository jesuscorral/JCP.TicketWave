using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.CatalogService.Domain.Models;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly CatalogDbContext _context;

    public EventRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Event>> GetAllAsync()
    {
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.CategoryId == categoryId)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetByVenueAsync(Guid venueId)
    {
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.VenueId == venueId)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int count = 10)
    {
        var now = DateTime.UtcNow;
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.StartDateTime > now && e.Status == EventStatus.Published)
            .OrderBy(e => e.StartDateTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var normalizedSearchTerm = searchTerm.ToLower().Trim();
        
        return await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Where(e => e.Title.ToLower().Contains(normalizedSearchTerm) ||
                       (e.Description != null && e.Description.ToLower().Contains(normalizedSearchTerm)) ||
                       e.Category!.Name.ToLower().Contains(normalizedSearchTerm) ||
                       e.Venue!.Name.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    public async Task<Event> AddAsync(Event entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Events.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Event> UpdateAsync(Event entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Events.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity != null)
        {
            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }
}