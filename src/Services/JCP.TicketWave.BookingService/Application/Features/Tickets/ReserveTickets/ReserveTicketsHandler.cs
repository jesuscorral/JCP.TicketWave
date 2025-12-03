using JCP.TicketWave.BookingService.Domain.Interfaces;
using JCP.TicketWave.BookingService.Domain.Models;

namespace JCP.TicketWave.BookingService.Application.Features.Tickets.ReserveTickets;

public class ReserveTicketsHandler
{
    private readonly ITicketRepository _ticketRepository;

    public ReserveTicketsHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task<ReserveTicketsResponse> Handle(ReserveTicketsCommand command, CancellationToken cancellationToken)
    {
        // Check ticket availability
        var availableTickets = await _ticketRepository.GetAvailableByEventIdAsync(command.EventId, null, cancellationToken);
        
        if (availableTickets.Count() < command.TicketCount)
        {
            return new ReserveTicketsResponse(
                ReservationId: Guid.Empty,
                ExpiresAt: DateTime.UtcNow,
                Success: false,
                ErrorMessage: "Insufficient tickets available"
            );
        }

        // Reserve tickets
        var reservationId = Guid.NewGuid();
        var reservationDuration = TimeSpan.FromMinutes(15);
        var ticketsToReserve = availableTickets.Take(command.TicketCount);

        foreach (var ticket in ticketsToReserve)
        {
            ticket.Reserve(reservationId, reservationDuration);
            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        }

        return new ReserveTicketsResponse(
            ReservationId: reservationId,
            ExpiresAt: DateTime.UtcNow.Add(reservationDuration),
            Success: true,
            ErrorMessage: null);
    }
}