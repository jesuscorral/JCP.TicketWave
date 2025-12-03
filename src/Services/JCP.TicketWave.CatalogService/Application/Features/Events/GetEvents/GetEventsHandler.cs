using JCP.TicketWave.CatalogService.Domain.Interfaces;

namespace JCP.TicketWave.CatalogService.Application.Features.Events.GetEvents;

public class GetEventsHandler
{
    private readonly IEventRepository _eventRepository;

    public GetEventsHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
    }

    public async Task<GetEventsResponse> Handle(GetEventsQuery query, CancellationToken cancellationToken)
    {
        // Get all events - the repository interface doesn't support paging yet
        var allEvents = await _eventRepository.GetAllAsync();
        
        // Filter by search term if provided
        if (!string.IsNullOrEmpty(query.Search))
        {
            allEvents = await _eventRepository.SearchEventsAsync(query.Search);
        }

        // Apply pagination manually for now
        var events = allEvents.Skip((query.Page - 1) * query.PageSize)
                             .Take(query.PageSize);

        var eventDtos = events.Select(e => new EventDto(
            Id: e.Id,
            Title: e.Title,
            Description: e.Description ?? string.Empty,
            StartDate: e.StartDateTime,
            EndDate: e.EndDateTime,
            Venue: e.Venue?.Name ?? "Unknown", // Using navigation property
            Category: e.Category?.Name ?? "Unknown", // Using navigation property
            Price: e.TicketPrice,
            AvailableTickets: e.AvailableTickets
        )).ToArray();

        return new GetEventsResponse(
            Events: eventDtos,
            TotalCount: allEvents.Count(),
            Page: query.Page,
            PageSize: query.PageSize);
    }
}