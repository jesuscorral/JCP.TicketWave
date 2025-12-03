using Microsoft.EntityFrameworkCore;
using JCP.TicketWave.CatalogService.Domain.Models;
using JCP.TicketWave.CatalogService.Domain.Interfaces;
using JCP.TicketWave.CatalogService.Infrastructure.Persistence;

namespace JCP.TicketWave.CatalogService.Infrastructure.Persistence.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly CatalogDbContext _context;

    public VenueRepository(CatalogDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Venue>> GetAllAsync()
    {
        return await _context.Venues
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue?> GetByIdAsync(Guid id)
    {
        return await _context.Venues
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Venue>> GetByCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            return new List<Venue>();

        var normalizedCity = city.ToLower().Trim();
        
        return await _context.Venues
            .Where(v => v.IsActive && v.CityId.ToLower() == normalizedCity)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Venue>> SearchVenuesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var normalizedSearchTerm = searchTerm.ToLower().Trim();
        
        return await _context.Venues
            .Where(v => v.IsActive && 
                       (v.Name.ToLower().Contains(normalizedSearchTerm) ||
                        v.Address.ToLower().Contains(normalizedSearchTerm) ||
                        (v.Description != null && v.Description.ToLower().Contains(normalizedSearchTerm))))
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Venue> AddAsync(Venue entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Venues.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Venue> UpdateAsync(Venue entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _context.Venues.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Venues.FindAsync(id);
        if (entity != null)
        {
            _context.Venues.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Venues.AnyAsync(v => v.Id == id);
    }
}