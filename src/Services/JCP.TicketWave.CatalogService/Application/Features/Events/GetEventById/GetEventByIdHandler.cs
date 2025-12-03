using JCP.TicketWave.CatalogService.Domain.Interfaces;

namespace JCP.TicketWave.CatalogService.Application.Features.Events.GetEventById;

public class GetEventByIdHandler
{
    private readonly IEventRepository _eventRepository;

    public GetEventByIdHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    }

    public async Task<GetEventByIdResponse?> Handle(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var eventEntity = await _eventRepository.GetByIdAsync(query.Id);
        
        if (eventEntity == null)
            return null;

        return new GetEventByIdResponse(
            Id: eventEntity.Id,
            Title: eventEntity.Title,
            Description: eventEntity.Description ?? string.Empty,
            StartDate: eventEntity.StartDateTime,
            EndDate: eventEntity.EndDateTime,
            Venue: eventEntity.Venue?.Name ?? "Unknown",
            Category: eventEntity.Category?.Name ?? "Unknown",
            Price: eventEntity.TicketPrice,
            AvailableTickets: eventEntity.AvailableTickets,
            ImageUrl: eventEntity.ImageUrl
        );
    }
}