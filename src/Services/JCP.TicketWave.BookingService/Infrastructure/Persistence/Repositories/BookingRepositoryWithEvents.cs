using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Domain.Models;
using JCP.TicketWave.Shared.Infrastructure.Events;

namespace JCP.TicketWave.BookingService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de Booking que incluye manejo automático de eventos de dominio
/// </summary>
public class BookingRepositoryWithEvents : IBookingRepository
{
    private readonly BookingRepository _baseRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public BookingRepositoryWithEvents(
        BookingDbContext context, 
        IDomainEventDispatcher eventDispatcher) 
    {
        _baseRepository = new BookingRepository(context);
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetByEventIdAsync(eventId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<Booking> CreateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var result = await _baseRepository.CreateAsync(booking, cancellationToken);
        
        // Despachar eventos de dominio después de persistir
        await _eventDispatcher.DispatchAndClearEventsAsync(booking, cancellationToken);
        
        return result;
    }

    public async Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        var result = await _baseRepository.UpdateAsync(booking, cancellationToken);
        
        // Despachar eventos de dominio después de actualizar
        await _eventDispatcher.DispatchAndClearEventsAsync(booking, cancellationToken);
        
        return result;
    }

    public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetByStatusAsync(status, cancellationToken);
    }

    public async Task<IEnumerable<Booking>> GetExpiredBookingsAsync(CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetExpiredBookingsAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.CountByUserIdAsync(userId, cancellationToken);
    }

    public async Task<int> CountByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _baseRepository.CountByEventIdAsync(eventId, cancellationToken);
    }

    public async Task<(IEnumerable<Booking> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? userId = null,
        Guid? eventId = null,
        BookingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        return await _baseRepository.GetPagedAsync(pageNumber, pageSize, userId, eventId, status, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Para delete, primero obtenemos el booking para despachar eventos si es necesario
        var booking = await GetByIdAsync(id, cancellationToken);
        if (booking != null)
        {
            await _eventDispatcher.DispatchAndClearEventsAsync(booking, cancellationToken);
        }
        
        await _baseRepository.DeleteAsync(id, cancellationToken);
    }
}